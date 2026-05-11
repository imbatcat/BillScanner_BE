using Business.Handlers.Bills.GetBillDetails.Dto;
using MediatR;

namespace Business.Handlers.Bills.GetBillDetails;

public record GetBillDetailsQuery(Guid UserId, Guid Id)
    : IRequest<GetBillDetailsResponse>;
