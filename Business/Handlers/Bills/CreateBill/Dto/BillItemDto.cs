namespace Business.Handlers.Bills.CreateBill.Dto
{
    public record BillItemDto
    {
        public string ItemName { get; init; } = null!;
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal TotalPrice { get; init; }
    }
}
