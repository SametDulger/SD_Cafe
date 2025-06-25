using SDCafe.DataAccess;
using SDCafe.Entities;

namespace SDCafe.Business
{
    public class PaymentService : Service<Payment>, IPaymentService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Table> _tableRepository;

        public PaymentService(
            IRepository<Payment> repository,
            IRepository<Order> orderRepository,
            IRepository<Table> tableRepository) : base(repository)
        {
            _orderRepository = orderRepository;
            _tableRepository = tableRepository;
        }

        public async Task<Payment> CreatePaymentAsync(int orderId, int cashierId, decimal amount, PaymentMethod paymentMethod)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            if (order.Status != OrderStatus.Completed)
                throw new InvalidOperationException("Order must be completed before payment");

            var payment = new Payment
            {
                OrderId = orderId,
                CashierId = cashierId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                Status = PaymentStatus.Pending,
                CreatedDate = DateTime.Now
            };

            return await _repository.AddAsync(payment);
        }

        public async Task<bool> CompletePaymentAsync(int paymentId)
        {
            var payment = await _repository.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            payment.Status = PaymentStatus.Completed;
            payment.UpdatedDate = DateTime.Now;
            payment.ReceiptNumber = $"RCP-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

            await _repository.UpdateAsync(payment);

            var order = await _orderRepository.GetByIdAsync(payment.OrderId);
            if (order != null)
            {
                var table = await _tableRepository.GetByIdAsync(order.TableId);
                if (table != null)
                {
                    table.IsOccupied = false;
                    await _tableRepository.UpdateAsync(table);
                }
            }

            return true;
        }

        public async Task<IEnumerable<Payment>> GetByOrderAsync(int orderId)
        {
            return await _repository.FindAsync(p => p.OrderId == orderId && !p.IsDeleted);
        }

        public async Task<IEnumerable<Payment>> GetByCashierAsync(int cashierId)
        {
            return await _repository.FindAsync(p => p.CashierId == cashierId && !p.IsDeleted);
        }

        public async Task<decimal> GetTotalPaymentsByOrderAsync(int orderId)
        {
            var payments = await GetByOrderAsync(orderId);
            return payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
        }

        public async Task<Payment?> GetByIdWithNestedIncludesAsync(int id, params string[] includes)
        {
            return await _repository.GetByIdWithNestedIncludesAsync(id, includes);
        }

        public new async Task<IEnumerable<Payment>> GetAllWithIncludesAsync(params System.Linq.Expressions.Expression<Func<Payment, object>>[] includes)
        {
            return await _repository.GetAllWithIncludesAsync(includes);
        }
    }
} 