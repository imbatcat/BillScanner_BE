namespace Business.Handlers.Bills.CreateBill.Dto
{
    public record UserEditsDto
    {
        public string MerchantName { get; init; } = null!;
        public string MerchantBank { get; init; } = null!;
        public string MerchantBankNumber { get; init; } = null!;
        public DateOnly BillDate { get; init; }
        public TimeSpan? BillTime { get; init; }
        public decimal? Total { get; init; }
        public decimal? SubTotal { get; init; }
        public decimal? Tax { get; init; }
        public string? Currency { get; init; }
        public List<BillItemDto> Items { get; init; } = [];
    }
}
