using System.ComponentModel.DataAnnotations;

namespace SDCafe.Entities
{
    public class Product : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
} 