namespace Business.Handlers.Images.ProcessImage.Dto.ImageProcessing
{
    public record ExtractedValue<T>
    {
        public T Value { get; set; } = default!;
        public decimal Confidence { get; set; } = 0m;
    }
}
