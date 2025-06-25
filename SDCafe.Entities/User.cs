using System.ComponentModel.DataAnnotations;

namespace SDCafe.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string Password { get; set; } = null!;
        
        [Required]
        public UserRole Role { get; set; }
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
    
    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        Waiter = 3,
        Cashier = 4,
        Kitchen = 5,
        Accounting = 6
    }
} 