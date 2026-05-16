using Business.Common;
using Business.Interfaces.Services;
using MediatR;

namespace Business.Handlers.Webhooks.FileUploadedWebhook
{
    public class FileUploadedWebhookHandler(
        IImageExtractionService imageExtractionService,
        ICachingService cachingService)
        : IRequestHandler<FileUploadedWebhookCommand>
    {
        public async Task Handle(FileUploadedWebhookCommand request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(request.UserId);
            var result = await imageExtractionService.ExtractImageAsync(request.Url, request.IsInvoice);

            var resultId = CacheKeys.StableIdFromUrl(request.Url);
            await cachingService.SetAsync(
                CacheKeys.GetProcessResultCacheKey(userId, resultId),
                new CachedOcrResult(request.Url, result));
        }
    }
}
