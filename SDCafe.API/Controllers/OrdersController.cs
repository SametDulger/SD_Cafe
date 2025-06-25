using Microsoft.AspNetCore.Mvc;
using SDCafe.Business;
using SDCafe.Entities;

namespace SDCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpGet("table/{tableId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByTable(int tableId)
        {
            var orders = await _orderService.GetByTableAsync(tableId);
            return Ok(orders);
        }

        [HttpGet("waiter/{waiterId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByWaiter(int waiterId)
        {
            var orders = await _orderService.GetByWaiterAsync(waiterId);
            return Ok(orders);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByStatus(OrderStatus status)
        {
            var orders = await _orderService.GetByStatusAsync(status);
            return Ok(orders);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Order>>> GetActiveOrders()
        {
            var orders = await _orderService.FindAsync(o => o.Status != OrderStatus.Completed && !o.IsDeleted);
            return Ok(orders);
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(request.TableId, request.WaiterId, request.Items);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatus status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, status);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("{id}/total")]
        public async Task<ActionResult<decimal>> GetOrderTotal(int id)
        {
            var total = await _orderService.CalculateTotalAsync(id);
            return Ok(total);
        }

        [HttpGet("table/{tableId}/occupied")]
        public async Task<ActionResult<bool>> IsTableOccupied(int tableId)
        {
            var isOccupied = await _orderService.IsTableOccupiedAsync(tableId);
            return Ok(isOccupied);
        }
    }

    public class CreateOrderRequest
    {
        public int TableId { get; set; }
        public int WaiterId { get; set; }
        public List<OrderItemDto> Items { get; set; } = null!;
    }
} 