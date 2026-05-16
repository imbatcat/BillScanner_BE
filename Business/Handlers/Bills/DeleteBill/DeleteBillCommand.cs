using MediatR;

namespace Business.Handlers.Bills.DeleteBill;

public record DeleteBillCommand(Guid UserId, Guid Id) : IRequest;
