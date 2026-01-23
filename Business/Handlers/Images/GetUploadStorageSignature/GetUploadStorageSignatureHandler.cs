using Business.Handlers.Images.GetUploadStorageSignature.Dto;
using Business.Interfaces.Services;
using MediatR;

namespace Business.Handlers.Images.GetUploadStorageSignature
{
    public class QueryHandler(
        IFileStorageService fileStorageService
    ) : IRequestHandler<GetUploadStorageSignatureRequest, GetUploadStorageSignatureResponse>
    {
        public Task<GetUploadStorageSignatureResponse> Handle(GetUploadStorageSignatureRequest request,
            CancellationToken cancellationToken)
        {
            return fileStorageService.GetUploadStorageSignatureAsync();
        }
    }
}