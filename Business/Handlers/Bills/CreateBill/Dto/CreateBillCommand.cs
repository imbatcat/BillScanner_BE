using Domain.Entities;
using MediatR;

namespace Business.Handlers.Bills.CreateBill.Dto;

public record CreateBillCommand : IRequest<CreateBillResponse>
{
    public Guid UserId { get; init; }
    public string? ImgUrl { get; init; }
    public ExtractionMethod ExtractionMethod { get; init; }
    public UserEditsDto UserEdits { get; init; } = null!;
}