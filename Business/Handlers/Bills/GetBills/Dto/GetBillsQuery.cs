using MediatR;

namespace Business.Handlers.Bills.GetBills.Dto
{
    public record GetBillsQuery(Guid UserId, BillParams Params)
        : IRequest<GetBillsResponse>;
}
