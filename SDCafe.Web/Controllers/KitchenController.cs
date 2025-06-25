using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SDCafe.Business;
using SDCafe.Entities;

namespace SDCafe.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager,Kitchen")]
    public class KitchenController : Controller
    {
        private readonly IOrderService _orderService;

        public KitchenController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var pendingOrders = await _orderService.GetByStatusAsync(OrderStatus.Pending);
            var preparingOrders = await _orderService.GetByStatusAsync(OrderStatus.Preparing);
            
            var allOrders = pendingOrders.Concat(preparingOrders).ToList();
            
            var ordersWithIncludes = new List<Order>();
            foreach (var order in allOrders)
            {
                var orderWithIncludes = await _orderService.GetByIdWithNestedIncludesAsync(order.Id, 
                    "Table", 
                    "Waiter", 
                    "OrderItems.Product");
                if (orderWithIncludes != null)
                {
                    ordersWithIncludes.Add(orderWithIncludes);
                }
            }
            
            return View(ordersWithIncludes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Preparing);
            if (success)
            {
                TempData["Success"] = "Sipariş kabul edildi ve hazırlanmaya başlandı.";
            }
            else
            {
                TempData["Error"] = "Sipariş durumu güncellenirken hata oluştu.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Ready);
            if (success)
            {
                TempData["Success"] = "Sipariş hazırlandı ve teslim için hazır.";
            }
            else
            {
                TempData["Error"] = "Sipariş durumu güncellenirken hata oluştu.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _orderService.GetByIdWithNestedIncludesAsync(id, 
                "Table", 
                "Waiter", 
                "OrderItems.Product");
            
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
} 