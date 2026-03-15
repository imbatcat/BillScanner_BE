
namespace Business.Handlers.Bills.CreateBill.Dto;

public record CreateBillResponse
{
    public Guid? BillId { get; init; }
}