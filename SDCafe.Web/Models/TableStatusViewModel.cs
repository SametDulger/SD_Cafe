using SDCafe.Entities;

namespace SDCafe.Web.Models
{
    public class TableStatusViewModel
    {
        public Table Table { get; set; } = null!;
        public Order? ActiveOrder { get; set; }
        public bool IsOccupied { get; set; }
    }
} 