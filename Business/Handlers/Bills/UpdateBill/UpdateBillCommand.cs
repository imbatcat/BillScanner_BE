using Business.Handlers.Bills.CreateBill.Dto;
using MediatR;

namespace Business.Handlers.Bills.UpdateBill;

public record UpdateBillCommand : IRequest
{
    public Guid UserId { get; init; }
    public Guid Id { get; init; }
    public UserEditsDto UserEdits { get; init; } = null!;
}
