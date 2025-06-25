using SDCafe.Entities;

namespace SDCafe.Web.Models
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public string SearchTerm { get; set; } = string.Empty;
        public int? SelectedCategoryId { get; set; }
    }
} 