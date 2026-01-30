namespace Business.Handlers.Images.ProcessImage.Dto.ImageProcessing
{
    public record ExtractedVendor
    {
        public ExtractedValue<string?> Name { get; set; } = new();
    }
}