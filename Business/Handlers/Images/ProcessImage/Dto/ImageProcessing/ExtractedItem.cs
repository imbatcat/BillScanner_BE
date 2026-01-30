namespace Business.Handlers.Images.ProcessImage.Dto.ImageProcessing
{
    public record ExtractedItem
    {
        public ExtractedValue<string?> ItemName { get; set; } = new();
        public ExtractedValue<decimal?> UnitPrice { get; set; } = new();
        public ExtractedValue<int?> Quantity { get; set; } = new();
        public ExtractedValue<decimal?> TotalPrice { get; set; } = new();
    }
}