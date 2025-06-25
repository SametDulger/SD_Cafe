using System.ComponentModel.DataAnnotations;

namespace SDCafe.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
} 