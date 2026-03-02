namespace Domain.Entities
{
    public class Bill : BaseEntity
    {
        // UI props
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public DateTime BillDate { get; set; }

        public string? MerchantName { get; set; }


        public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = [];

        public ICollection<BillItem> BillItems { get; set; } = [];

        // System related props

        public ExtractionMethod ExtractionMethod { get; set; }

        public BillStatus Status { get; set; } = BillStatus.Pending;
    }

    public enum ExtractionMethod
    {
        Qr,

        Ocr,

        Manual
    }

    public enum BillStatus
    {
        Pending,

        Verified,

        Edited,

        Exported
    }
}