using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    internal class Bill : BaseEntity
    {
        public Guid UserId { get; set; }
        public string ExtractionMethod { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public decimal PaymentAmount { get; set; }
        public string Currency { get; set; } = "VND";
        public DateTime BillDate { get; set; }
        public int ConfidenceScore { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
        public ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();
    }
}
