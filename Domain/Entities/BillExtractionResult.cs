namespace Domain.Entities
{
    public class BillExtractionResult : BaseEntity
    {
        public Guid BillId { get; set; }

        public string? ExtractedMerchantName { get; set; }
        public decimal? MerchantNameConfidence { get; set; }
        public bool? IsMerchantNameCorrect { get; set; }

        public decimal? ExtractedTotalAmount { get; set; }
        public decimal? TotalAmountConfidence { get; set; }
        public bool? IsTotalAmountCorrect { get; set; }

        public DateTime? ExtractedBillDate { get; set; }
        public decimal? BillDateConfidence { get; set; }
        public bool? IsBillDateCorrect { get; set; }
        public string? ExtractedCurrency { get; set; }
        public decimal? CurrencyConfidence { get; set; }
        public bool? IsCurrencyCorrect { get; set; }

        public Bill Bill { get; set; } = null!;
        public ICollection<BillItemExtractionResult> BillItemExtractionResults { get; set; } = [];
    }
}
