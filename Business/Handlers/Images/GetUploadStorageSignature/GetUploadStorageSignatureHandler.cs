using Business.Handlers.Images.GetUploadStorageSignature.Dto;
using Business.Interfaces.Services;
using MediatR;

namespace Business.Handlers.Images.GetUploadStorageSignature
{
    public class QueryHandler(
        IFileStorageService _fileStorageService
    ) : IRequestHandler<GetUploadStorageSignatureRequest, GetUploadStorageSignatureResponse>
    {
        public async Task<GetUploadStorageSignatureResponse> Handle(GetUploadStorageSignatureRequest request,
            CancellationToken cancellationToken)
        {
            return await _fileStorageService.GetUploadStorageSignatureAsync(
                request.IsInvoice,
                request.UserId);
        }
    }
}