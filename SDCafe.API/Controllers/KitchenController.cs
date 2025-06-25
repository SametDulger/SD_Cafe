using Microsoft.AspNetCore.Mvc;
using SDCafe.Business;
using SDCafe.Entities;

namespace SDCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KitchenController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public KitchenController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetKitchenOrders()
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
            
            return Ok(ordersWithIncludes);
        }

        [HttpPost("accept/{orderId}")]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Preparing);
            if (success)
            {
                return Ok(new { message = "Sipariş kabul edildi ve hazırlanmaya başlandı." });
            }
            
            return BadRequest(new { message = "Sipariş durumu güncellenirken hata oluştu." });
        }

        [HttpPost("complete/{orderId}")]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Ready);
            if (success)
            {
                return Ok(new { message = "Sipariş hazırlandı ve teslim için hazır." });
            }
            
            return BadRequest(new { message = "Sipariş durumu güncellenirken hata oluştu." });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderDetails(int id)
        {
            var order = await _orderService.GetByIdWithNestedIncludesAsync(id, 
                "Table", 
                "Waiter", 
                "OrderItems.Product");
            
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }
    }
} 