namespace Domain.Entities
{
    public class Bill : BaseEntity
    {
        // UI props
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public DateOnly BillDate { get; set; }

        public TimeSpan? BillTime { get; set; }

        public decimal Discount { get; set; }
        public string? MerchantName { get; set; }

        public string? MerchantBankName { get; set; }

        public string? MerchantBankNumber { get; set; }

        public string Currency { get; set; } = "VND";

        public decimal? SubTotal { get; set; }

        public decimal? Tax { get; set; }

        public decimal? Total { get; set; }


        public ICollection<BillItem> BillItems { get; set; } = [];

        // System related props

        public string? ImgUrl { get; set; }

        public ExtractionMethod ExtractionMethod { get; set; }
    }

    public enum ExtractionMethod
    {
        Ocr,
        Manual
    }
}