using System.ComponentModel.DataAnnotations;
using SDCafe.Entities;

namespace SDCafe.Web.Models
{
    public class PaymentCreateViewModel
    {
        public int OrderId { get; set; }
        public int CashierId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? PaymentMethods { get; set; }
        public Order? Order { get; set; }
    }
} 