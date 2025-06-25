using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDCafe.Business;
using SDCafe.Entities;
using SDCafe.Web.Models;

namespace SDCafe.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager,Accounting")]
    public class AccountingController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        public AccountingController(IPaymentService paymentService, IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var paidOrders = await _orderService.GetAllWithIncludesAsync(o => o.Table, o => o.Waiter, o => o.Payments);
            var ordersWithPayments = paidOrders.Where(o => o.Payments.Any()).ToList();
            
            var allPayments = await _paymentService.GetAllWithIncludesAsync(p => p.Cashier);
            var paymentDict = allPayments.ToDictionary(p => p.Id, p => p.Cashier);
            
            foreach (var order in ordersWithPayments)
            {
                foreach (var payment in order.Payments)
                {
                    if (paymentDict.ContainsKey(payment.Id))
                    {
                        payment.Cashier = paymentDict[payment.Id];
                    }
                }
            }
            
            var totalRevenue = ordersWithPayments.Sum(o => o.TotalAmount);
            
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = ordersWithPayments.Count;
            ViewBag.TotalPaymentCount = ordersWithPayments.Sum(o => o.Payments.Count);
            
            return View(ordersWithPayments);
        }

        public async Task<IActionResult> PaymentReport()
        {
            var payments = await _paymentService.GetAllWithIncludesAsync(p => p.Cashier, p => p.Order);
            var paymentsWithDetails = new List<PaymentReportViewModel>();
            
            foreach (var payment in payments)
            {
                var order = await _orderService.GetByIdWithNestedIncludesAsync(payment.OrderId, 
                    "Table", 
                    "Waiter", 
                    "OrderItems.Product");
                
                if (order != null)
                {
                    paymentsWithDetails.Add(new PaymentReportViewModel
                    {
                        Payment = payment,
                        Order = order
                    });
                }
            }
            
            return View(paymentsWithDetails.OrderByDescending(p => p.Payment.CreatedDate));
        }

        public async Task<IActionResult> DailyReport(DateTime? date = null)
        {
            var targetDate = date ?? DateTime.Today;
            var startDate = targetDate.Date;
            var endDate = startDate.AddDays(1);
            
            var payments = await _paymentService.GetAllAsync();
            var dailyPayments = payments.Where(p => p.CreatedDate >= startDate && p.CreatedDate < endDate).ToList();
            
            var dailyOrders = await _orderService.GetAllWithIncludesAsync(o => o.Table, o => o.Waiter, o => o.Payments);
            var dailyPaidOrders = dailyOrders.Where(o => o.Payments.Any(p => p.CreatedDate >= startDate && p.CreatedDate < endDate)).ToList();
            
            // Payment'ların Cashier bilgisini yükle
            var allPayments = await _paymentService.GetAllWithIncludesAsync(p => p.Cashier);
            var paymentDict = allPayments.ToDictionary(p => p.Id, p => p.Cashier);
            
            foreach (var order in dailyPaidOrders)
            {
                foreach (var payment in order.Payments)
                {
                    if (paymentDict.ContainsKey(payment.Id))
                    {
                        payment.Cashier = paymentDict[payment.Id];
                    }
                }
            }
            
            ViewBag.TargetDate = targetDate;
            ViewBag.DailyRevenue = dailyPaidOrders.Sum(o => o.TotalAmount);
            ViewBag.DailyPaymentCount = dailyPayments.Count;
            ViewBag.DailyOrderCount = dailyPaidOrders.Count;
            
            return View(dailyPaidOrders);
        }
    }
} 