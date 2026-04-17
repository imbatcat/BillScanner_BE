namespace Domain.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid BillId { get; set; }

        public Guid? BankId { get; set; }

        public Bank? Bank { get; set; }

        public string? BankNumber { get; set; }

        public string? AccountHolder { get; set; } // the one receiving the payment
        
        public decimal TransactionAmount { get; set; }

        public string Currency { get; set; } = "VND";

        public string? PaymentContent { get; set; }

        public Bill Bill { get; set; } = null!;
    }


}
