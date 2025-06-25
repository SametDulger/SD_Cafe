using System.ComponentModel.DataAnnotations;

namespace SDCafe.Entities
{
    public class Table : BaseEntity
    {
        [Required]
        [StringLength(20)]
        public string TableNumber { get; set; } = null!;
        
        [Required]
        [Range(1, 20)]
        public int Capacity { get; set; }
        
        public bool IsOccupied { get; set; } = false;
        
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
} 