using MediatR;

namespace Business.Handlers.Images.GetUploadStorageSignature.Dto
{
    public record GetUploadStorageSignatureRequest(
        bool IsInvoice,
        Guid UserId
    ) : IRequest<GetUploadStorageSignatureResponse>;
}