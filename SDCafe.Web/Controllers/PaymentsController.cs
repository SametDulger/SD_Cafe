using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using SDCafe.Business;
using SDCafe.Entities;
using SDCafe.Web.Models;

namespace SDCafe.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager,Cashier,Accounting")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IService<User> _userService;

        public PaymentsController(
            IPaymentService paymentService,
            IOrderService orderService,
            IService<User> userService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllAsync();
            
            var paidOrders = await _orderService.GetAllWithIncludesAsync(o => o.Table, o => o.Waiter, o => o.Payments);
            var ordersWithPayments = paidOrders.Where(o => o.Payments.Any()).ToList();
            
            ViewBag.PaidOrders = ordersWithPayments;
            return View(payments);
        }

        public async Task<IActionResult> Create(int orderId)
        {
            var order = await _orderService.GetByIdWithNestedIncludesAsync(orderId, 
                "Table",
                "Waiter",
                "OrderItems.Product");

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Completed)
            {
                TempData["Error"] = "Sadece tamamlanmış siparişler için ödeme alınabilir.";
                return RedirectToAction("Index", "Orders");
            }

            var currentUserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var currentUser = await _userService.FindAsync(u => u.Email == currentUserEmail);
            var cashier = currentUser.FirstOrDefault();

            var viewModel = new PaymentCreateViewModel
            {
                OrderId = orderId,
                Order = order,
                CashierId = cashier?.Id ?? 0,
                Amount = order.TotalAmount,
                PaymentMethods = Enum.GetValues<PaymentMethod>().Select(pm => new SelectListItem
                {
                    Value = ((int)pm).ToString(),
                    Text = GetPaymentMethodText(pm)
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var payment = await _paymentService.CreatePaymentAsync(
                        viewModel.OrderId, 
                        viewModel.CashierId, 
                        viewModel.Amount, 
                        viewModel.PaymentMethod);

                    await _paymentService.CompletePaymentAsync(payment.Id);

                    TempData["Success"] = "Ödeme başarıyla tamamlandı. Masa boşaltıldı.";
                    return RedirectToAction("Receipt", new { id = payment.Id });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var order = await _orderService.GetByIdWithNestedIncludesAsync(viewModel.OrderId, 
                "Table", 
                "Waiter", 
                "OrderItems.Product");
            
            viewModel.Order = order;
            viewModel.PaymentMethods = Enum.GetValues<PaymentMethod>().Select(pm => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = ((int)pm).ToString(),
                Text = GetPaymentMethodText(pm)
            }).ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var payment = await _paymentService.GetByIdWithNestedIncludesAsync(id, 
                "Order.Table", 
                "Order.Waiter", 
                "Order.OrderItems.Product", 
                "Cashier");
            
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        private string GetPaymentMethodText(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "Nakit",
                PaymentMethod.CreditCard => "Kredi Kartı",
                PaymentMethod.DebitCard => "Banka Kartı",
                PaymentMethod.MobilePayment => "Mobil Ödeme",
                _ => method.ToString()
            };
        }
    }
} 