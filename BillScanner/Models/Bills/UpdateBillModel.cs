using Business.Handlers.Bills.CreateBill.Dto;

namespace BillScanner.Models.Bills;

public record UpdateBillModel
{
    public UserEditsDto UserEdits { get; init; } = null!;
}
