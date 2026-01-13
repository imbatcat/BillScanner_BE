using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    internal class PaymentMethod : BaseEntity
    {
        public Guid BillId { get; set; }
        public string MethodType { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = string.Empty;
        public decimal TransactionAmount { get; set; }
        public string Currency { get; set; } = "VND";
        public string PaymentContent { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }

        public Bill Bill { get; set; } = null!;
    }
}
