using Business.Common;
using Business.Handlers.Webhooks.FileUploadedWebhook.Dto;
using Business.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Options;

namespace Business.Handlers.Webhooks.FileUploadedWebhook
{
    public class FileUploadedWebhookHandler(
        IImageExtractionService imageExtractionService,
        ICachingService cachingService,
        IOptions<BusinessSettings> settings)
        : IRequestHandler<FileUploadedWebhookCommand>
    {
        public async Task Handle(FileUploadedWebhookCommand request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(request.UserId);
            var result = await imageExtractionService.ExtractImageAsync(request.Url, request.IsInvoice);

            await cachingService.SetAsync(
                CacheKeys.GetImageUrlCacheKey(request.PublicId),
                new ImageUploadCacheEntry(userId, request.Url),
                TimeSpan.FromMinutes(settings.Value.CacheExpirationTimeInMinutes));

            await cachingService.SetAsync(
                CacheKeys.GetProcessResultCacheKey(userId, request.Url),
                result);
        }
    }
}
