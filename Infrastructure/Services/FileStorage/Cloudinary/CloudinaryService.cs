using Business.Handlers.Images.GetUploadStorageSignature.Dto;
using Business.Interfaces.Services;
using CloudinaryDotNet.Actions;
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
        public async Task DeleteImageAsync(string publicId)
        {
            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }

        public Task<GetUploadStorageSignatureResponse> GetUploadStorageSignatureAsync(bool isInvoice, Guid userId)
        {
            var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            var folder = isInvoice
                ? $"{_configuration.Value.InvoiceFolderName}"
                : _configuration.Value.ReceiptFolderName;
            var parameters = new SortedDictionary<string, object>
            {
                { "context", $"user_id={userId}|is_invoice={isInvoice.ToString().ToLower()}" },
                { "asset_folder", folder },
                { "timestamp", timestamp },
            };

            var signature = _cloudinary.Api.SignParameters(parameters);

            return Task.FromResult(new GetUploadStorageSignatureResponse(
                Signature: signature,
                Timestamp: timestamp,
                ApiKey: _cloudinary.Api.Account.ApiKey,
                Folder: folder
            ));
        }
    }
}