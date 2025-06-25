using SDCafe.Entities;

namespace SDCafe.Web.Models
{
    public class PaymentReportViewModel
    {
        public Payment Payment { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
} 