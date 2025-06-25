using System.ComponentModel.DataAnnotations;

namespace SDCafe.Entities
{
    public class Payment : BaseEntity
    {
        [Required]
        public string PaymentNumber { get; set; } = GeneratePaymentNumber();
        
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        
        public int? CashierId { get; set; }
        public virtual User Cashier { get; set; } = null!;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        
        public string? ReceiptNumber { get; set; }
        
        private static string GeneratePaymentNumber()
        {
            return $"PAY-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
    
    public enum PaymentMethod
    {
        Cash = 1,
        CreditCard = 2,
        DebitCard = 3,
        MobilePayment = 4
    }
    
    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4
    }
} 