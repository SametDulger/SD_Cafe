using SDCafe.Entities;
using System.Linq.Expressions;

namespace SDCafe.Business
{
    public interface IPaymentService : IService<Payment>
    {
        Task<Payment> CreatePaymentAsync(int orderId, int cashierId, decimal amount, PaymentMethod paymentMethod);
        Task<bool> CompletePaymentAsync(int paymentId);
        Task<Payment?> GetByIdWithNestedIncludesAsync(int id, params string[] includes);
        new Task<IEnumerable<Payment>> GetAllWithIncludesAsync(params Expression<Func<Payment, object>>[] includes);
        Task<IEnumerable<Payment>> GetByOrderAsync(int orderId);
        Task<IEnumerable<Payment>> GetByCashierAsync(int cashierId);
        Task<decimal> GetTotalPaymentsByOrderAsync(int orderId);
    }
} 