using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using SDCafe.Business;
using SDCafe.Entities;

namespace SDCafe.Web.Models
{
    public class OrderCreateViewModel
    {
        [Required(ErrorMessage = "Masa seçimi zorunludur")]
        [Display(Name = "Masa")]
        public int TableId { get; set; }

        [Required(ErrorMessage = "Garson seçimi zorunludur")]
        [Display(Name = "Garson")]
        public int WaiterId { get; set; }

        [Display(Name = "Notlar")]
        public string? Notes { get; set; }

        public List<SelectListItem> Tables { get; set; } = new();
        public List<SelectListItem> Waiters { get; set; } = new();
        public List<ProductSelectItem> Products { get; set; } = new();
    }

    public class ProductSelectItem : SelectListItem
    {
        public decimal Price { get; set; }
    }
} 