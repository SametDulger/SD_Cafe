using Microsoft.AspNetCore.Mvc;
using SDCafe.Business;
using SDCafe.Entities;

namespace SDCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountingController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        public AccountingController(IPaymentService paymentService, IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<AccountingIndexResponse>> GetAccountingIndex()
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
            
            var response = new AccountingIndexResponse
            {
                Orders = ordersWithPayments,
                TotalRevenue = ordersWithPayments.Sum(o => o.TotalAmount),
                TotalOrders = ordersWithPayments.Count,
                TotalPaymentCount = ordersWithPayments.Sum(o => o.Payments.Count)
            };
            
            return Ok(response);
        }

        [HttpGet("payment-report")]
        public async Task<ActionResult<IEnumerable<PaymentReportResponse>>> GetPaymentReport()
        {
            var payments = await _paymentService.GetAllWithIncludesAsync(p => p.Cashier, p => p.Order);
            var paymentsWithDetails = new List<PaymentReportResponse>();
            
            foreach (var payment in payments)
            {
                var order = await _orderService.GetByIdWithNestedIncludesAsync(payment.OrderId, 
                    "Table", 
                    "Waiter", 
                    "OrderItems.Product");
                
                if (order != null)
                {
                    paymentsWithDetails.Add(new PaymentReportResponse
                    {
                        Payment = payment,
                        Order = order
                    });
                }
            }
            
            return Ok(paymentsWithDetails.OrderByDescending(p => p.Payment.CreatedDate));
        }

        [HttpGet("daily-report")]
        public async Task<ActionResult<DailyReportResponse>> GetDailyReport(DateTime? date = null)
        {
            var targetDate = date ?? DateTime.Today;
            var startDate = targetDate.Date;
            var endDate = startDate.AddDays(1);
            
            var payments = await _paymentService.GetAllAsync();
            var dailyPayments = payments.Where(p => p.CreatedDate >= startDate && p.CreatedDate < endDate).ToList();
            
            var dailyOrders = await _orderService.GetAllWithIncludesAsync(o => o.Table, o => o.Waiter, o => o.Payments);
            var dailyPaidOrders = dailyOrders.Where(o => o.Payments.Any(p => p.CreatedDate >= startDate && p.CreatedDate < endDate)).ToList();
            
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
            
            var response = new DailyReportResponse
            {
                Orders = dailyPaidOrders,
                TargetDate = targetDate,
                DailyRevenue = dailyPaidOrders.Sum(o => o.TotalAmount),
                DailyPaymentCount = dailyPayments.Count,
                DailyOrderCount = dailyPaidOrders.Count
            };
            
            return Ok(response);
        }
    }

    public class AccountingIndexResponse
    {
        public List<Order> Orders { get; set; } = null!;
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalPaymentCount { get; set; }
    }

    public class PaymentReportResponse
    {
        public Payment Payment { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }

    public class DailyReportResponse
    {
        public List<Order> Orders { get; set; } = null!;
        public DateTime TargetDate { get; set; }
        public decimal DailyRevenue { get; set; }
        public int DailyPaymentCount { get; set; }
        public int DailyOrderCount { get; set; }
    }
} 