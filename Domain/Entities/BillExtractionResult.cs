namespace Domain.Entities
{
    public class BillExtractionResult : BaseEntity
    {
        public Guid BillId { get; set; }
        public Bill Bill { get; set; } = null!;

        public string? ExtractedMerchantName { get; set; }
        public decimal? MerchantNameConfidence { get; set; }
        public bool? IsMerchantNameCorrect { get; set; }
        public DateOnly? ExtractedBillDate { get; set; }
        public decimal? BillDateConfidence { get; set; }
        public bool? IsBillDateCorrect { get; set; }
        public TimeSpan? ExtractedBillTime { get; set; }
        public decimal? BillTimeConfidence { get; set; }
        public bool? IsBillTimeCorrect { get; set; }
        public string? ExtractedCurrency { get; set; }
        public decimal? CurrencyConfidence { get; set; }
        public bool? IsCurrencyCorrect { get; set; }

        public string? ExtractedPaymentType { get; set; }
        public decimal? PaymentTypeConfidence { get; set; }
        public bool? IsPaymentTypeCorrect { get; set; }

        public decimal? ExtractedTransactionAmount { get; set; }
        public decimal? TransactionAmountConfidence { get; set; }
        public bool? IsTransactionAmountCorrect { get; set; }

        public ICollection<BillItemExtractionResult> BillItemExtractionResults { get; set; } = [];
    }
}
