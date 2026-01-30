namespace Domain.Entities
{
    public class BillItemExtractionResult : BaseEntity
    {
        public Guid BillExtractionResultId { get; set; }

        public string? ExtractedItemName { get; set; }
        public decimal? ItemNameConfidence { get; set; }
        public bool? IsItemNameCorrect { get; set; }

        public decimal? ExtractedQuantity { get; set; }
        public decimal? QuantityConfidence { get; set; }
        public bool? IsQuantityCorrect { get; set; }

        public decimal? ExtractedUnitPrice { get; set; }
        public decimal? UnitPriceConfidence { get; set; }
        public bool? IsUnitPriceCorrect { get; set; }

        public decimal? ExtractedTotalPrice { get; set; }
        public decimal? TotalPriceConfidence { get; set; }
        public bool? IsTotalPriceCorrect { get; set; }

        public BillExtractionResult BillExtractionResult { get; set; } = null!;
    }
}
