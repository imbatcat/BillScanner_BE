using Business.Common;
using Business.Handlers.Webhooks.FileUploadedWebhook.Dto;
using Business.Interfaces.Services;
using MediatR;

namespace Business.Handlers.Webhooks.DeleteBillWebhook
{
    public class DeleteBillWebhookHandler(
        ICachingService cachingService)
        : IRequestHandler<DeleteBillWebhookCommand>
    {
        public async Task Handle(DeleteBillWebhookCommand request, CancellationToken cancellationToken)
        {
            var imageKey = CacheKeys.GetImageUrlCacheKey(request.PublicId);
            var entry = await cachingService.GetAsync<ImageUploadCacheEntry>(imageKey);

            if (entry is null) return;

            await cachingService.RemoveAsync(
                CacheKeys.GetProcessResultCacheKey(entry.UserId, entry.SecureUrl));

            await cachingService.RemoveAsync(imageKey);
        }
    }
}
