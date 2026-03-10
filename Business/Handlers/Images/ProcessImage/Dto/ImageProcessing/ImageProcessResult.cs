namespace Business.Handlers.Images.ProcessImage.Dto.ImageProcessing
{
    public record ImageProcessResult
    {
        public List<ExtractedItem> Items { get; set; } = [];
        public ExtractedVendor Vendor { get; set; } = new();
        public ExtractedValue<decimal?> SubTotal { get; set; } = new();
        public ExtractedValue<decimal?> Tax { get; set; } = new();
        public ExtractedValue<decimal?> Total { get; set; } = new();
        public ExtractedValue<string?> Currency { get; set; } = new();
        public ExtractedValue<DateOnly?> BillDate { get; set; } = new();
        public ExtractedValue<TimeSpan?> BillTime { get; set; } = new();
    }
}