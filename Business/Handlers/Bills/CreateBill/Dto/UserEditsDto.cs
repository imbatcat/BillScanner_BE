namespace Business.Handlers.Bills.Dto
{
    public record UserEditsDto
    {
        public string MerchantName { get; init; } = null!;
        public DateOnly BillDate { get; init; }
        public TimeSpan? BillTime { get; init; }
        public decimal? Total { get; init; }
        public decimal? SubTotal { get; init; }
        public decimal? Tax { get; init; }
        public List<BillItemDto> Items { get; init; } = [];
    }
}
