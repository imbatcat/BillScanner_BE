using Business.Handlers.Images.GetUploadStorageSignature.Dto;

namespace Business.Interfaces.Services
{
    public interface IFileStorageService
    {
        public Task<GetUploadStorageSignatureResponse> GetUploadStorageSignatureAsync(bool isInvoice, Guid userId);
    }
}