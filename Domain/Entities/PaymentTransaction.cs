namespace Domain.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid BillId { get; set; }

        public PaymentType PaymentType { get; set; }

        public Guid? BankId { get; set; }

        public Bank? Bank { get; set; }

        public string? BankAccount { get; set; }

        public string? AccountHolder { get; set; } // the one receiving the payment

        public decimal TransactionAmount { get; set; }

        public string Currency { get; set; } = "VND";

        public string? PaymentContent { get; set; }

        public Bill Bill { get; set; } = null!;
    }

    public enum PaymentType
    {
        VietQrStatic,

        VietQrDynamic,

        VietQrSemiDynamic,

        MomoQr,

        ZaloPayQr,

        VnPayQr,

        BankTransfer,

        Cash
    }
}
