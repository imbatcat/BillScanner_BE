namespace Business.Handlers.Bills.CreateBill.Dto
{
    public record UserEditsDto
    {
        public string MerchantName { get; init; } = null!;
        public string? MerchantBankName { get; init; }
        public string? MerchantBankNumber { get; init; }
        public DateOnly BillDate { get; init; }
        public TimeSpan? BillTime { get; init; }
        public decimal? Total { get; init; }
        public decimal? SubTotal { get; init; }
        public decimal? Tax { get; init; }
        public string? Currency { get; init; }
        public List<BillItemDto> Items { get; init; } = [];
    }
}
