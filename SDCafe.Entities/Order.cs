using System.ComponentModel.DataAnnotations;

namespace SDCafe.Entities
{
    public class Order : BaseEntity
    {
        [Required]
        public string OrderNumber { get; set; } = GenerateOrderNumber();
        
        public int TableId { get; set; }
        public virtual Table Table { get; set; } = null!;
        
        public int? WaiterId { get; set; }
        public virtual User Waiter { get; set; } = null!;
        
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        
        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
    
    public enum OrderStatus
    {
        Pending = 1,
        Preparing = 2,
        Ready = 3,
        Completed = 4,
        Cancelled = 5
    }
} 