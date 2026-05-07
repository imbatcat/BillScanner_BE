namespace Business.Handlers.Bills.GetBills.Dto
{
    public record GetBillsResponse(IReadOnlyList<BillDto> Items, int Total);
}
