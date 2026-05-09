using Domain.Entities;

namespace Business.Handlers.Bills.GetBills.Dto
{
    public record BillDto
    {
        public Guid Id { get; init; }
        public DateOnly BillDate { get; init; }
        public TimeSpan? BillTime { get; init; }
        public string? MerchantName { get; init; }
        public string Currency { get; init; } = "VND";
        public decimal? Total { get; init; }
        public string? ImgUrl { get; init; }
        public ExtractionMethod ExtractionMethod { get; init; }
    }
}
