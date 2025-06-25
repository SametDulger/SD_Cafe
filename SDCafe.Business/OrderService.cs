using SDCafe.DataAccess;
using SDCafe.Entities;
using System.Linq.Expressions;

namespace SDCafe.Business
{
    public class OrderService : Service<Order>, IOrderService
    {
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Table> _tableRepository;

        public OrderService(
            IRepository<Order> repository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Product> productRepository,
            IRepository<Table> tableRepository) : base(repository)
        {
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _tableRepository = tableRepository;
        }

        public override async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _repository.FindAsync(o => !o.IsDeleted);
        }

        public new async Task<IEnumerable<Order>> GetAllWithIncludesAsync(params Expression<Func<Order, object>>[] includes)
        {
            return await _repository.GetAllWithIncludesAsync(includes);
        }

        public override async Task<Order?> GetByIdWithIncludesAsync(int id, params Expression<Func<Order, object>>[] includes)
        {
            var result = await _repository.GetByIdWithIncludesAsync(id, includes);
            return result;
        }

        public async Task<Order?> GetByIdWithNestedIncludesAsync(int id, params string[] includes)
        {
            return await _repository.GetByIdWithNestedIncludesAsync(id, includes);
        }

        public async Task<Order> CreateOrderAsync(int tableId, int waiterId, List<OrderItemDto> items, string? notes = null)
        {
            if (await IsTableOccupiedAsync(tableId))
                throw new InvalidOperationException("Table is already occupied");

            var order = new Order
            {
                TableId = tableId,
                WaiterId = waiterId,
                Status = OrderStatus.Pending,
                Notes = notes,
                CreatedDate = DateTime.Now
            };

            order = await _repository.AddAsync(order);

            foreach (var item in items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product not found with ID {item.ProductId}");

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * item.Quantity,
                    Notes = item.Notes
                };

                await _orderItemRepository.AddAsync(orderItem);
            }

            order.TotalAmount = await CalculateTotalAsync(order.Id);
            await _repository.UpdateAsync(order);

            var table = await _tableRepository.GetByIdAsync(tableId);
            if (table != null)
            {
                table.IsOccupied = true;
                await _tableRepository.UpdateAsync(table);
            }

            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _repository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                order.UpdatedDate = DateTime.Now;
                
                if (status == OrderStatus.Completed)
                    order.CompletedDate = DateTime.Now;

                await _repository.UpdateAsync(order);
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Order>> GetByTableAsync(int tableId)
        {
            return await _repository.FindAsync(o => o.TableId == tableId && !o.IsDeleted);
        }

        public async Task<IEnumerable<Order>> GetByWaiterAsync(int waiterId)
        {
            return await _repository.FindAsync(o => o.WaiterId == waiterId && !o.IsDeleted);
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            return await _repository.FindAsync(o => o.Status == status && !o.IsDeleted);
        }

        public async Task<decimal> CalculateTotalAsync(int orderId)
        {
            var items = await _orderItemRepository.FindAsync(oi => oi.OrderId == orderId);
            return items.Sum(oi => oi.TotalPrice);
        }

        public async Task<bool> IsTableOccupiedAsync(int tableId)
        {
            var table = await _tableRepository.GetByIdAsync(tableId);
            return table?.IsOccupied ?? false;
        }
    }
} 