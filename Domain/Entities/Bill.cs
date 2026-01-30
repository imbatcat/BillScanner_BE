namespace Domain.Entities
{
    public class Bill : BaseEntity
    {
        public Guid UserId { get; set; }

        public ExtractionMethod ExtractionMethod { get; set; }

        public string? MerchantName { get; set; }

        public decimal PaymentAmount { get; set; }

        public string Currency { get; set; } = "VND";

        public DateTime BillDate { get; set; }

        public BillStatus Status { get; set; } = BillStatus.Pending;

        public User User { get; set; } = null!;

        public ICollection<PaymentMethod> PaymentMethods { get; set; } = [];

        public ICollection<BillItem> BillItems { get; set; } = [];
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