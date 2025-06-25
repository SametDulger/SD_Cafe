using SDCafe.Entities;
using System.Linq.Expressions;

namespace SDCafe.Business
{
    public interface IOrderService : IService<Order>
    {
        new Task<Order?> GetByIdWithIncludesAsync(int id, params Expression<Func<Order, object>>[] includes);
        Task<Order?> GetByIdWithNestedIncludesAsync(int id, params string[] includes);
        Task<Order> CreateOrderAsync(int tableId, int waiterId, List<OrderItemDto> items, string? notes = null);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetByTableAsync(int tableId);
        Task<IEnumerable<Order>> GetByWaiterAsync(int waiterId);
        new Task<IEnumerable<Order>> GetAllWithIncludesAsync(params Expression<Func<Order, object>>[] includes);
        Task<decimal> CalculateTotalAsync(int orderId);
        Task<bool> IsTableOccupiedAsync(int tableId);
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
} 