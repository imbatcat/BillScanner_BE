using Business.Handlers.Images.GetUploadStorageSignature.Dto;
using Business.Interfaces.Services;
using Infrastructure.MarkerInterfaces;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.FileStorage.Cloudinary
{
    [UsedImplicitly]
    public sealed class CloudinaryService(
        IOptions<CloudinarySettings> _configuration,
        CloudinaryDotNet.Cloudinary _cloudinary) : IFileStorageService, ISingletonService
    {
        public Task<GetUploadStorageSignatureResponse> GetUploadStorageSignatureAsync(bool isInvoice)
        {
            var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var folder = isInvoice ? $"{_configuration.Value.FolderName}/invoices" : _configuration.Value.FolderName;
            var parameters = new Dictionary<string, object>
            {
                { "folder", folder },
                { "timestamp", timestamp }
            };

            var signature = _cloudinary.Api.SignParameters(parameters);

            return Task.FromResult(new GetUploadStorageSignatureResponse(
                Signature: signature,
                Timestamp: timestamp,
                ApiKey: _cloudinary.Api.Account.ApiKey,
                CloudName: _cloudinary.Api.Account.Cloud
            ));
        }
    }
}