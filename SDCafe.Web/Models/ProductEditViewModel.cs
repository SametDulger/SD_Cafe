using SDCafe.Entities;

namespace SDCafe.Web.Models
{
    public class ProductEditViewModel
    {
        public Product Product { get; set; } = new Product();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    }
} 