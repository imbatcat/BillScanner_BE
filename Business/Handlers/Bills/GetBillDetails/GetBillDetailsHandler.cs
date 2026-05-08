using Business.Common;
using Business.Handlers.Bills.GetBillDetails.Dto;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Business.Handlers.Webhooks.FileUploadedWebhook.Dto;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Business.Specifications.Bills;
using Domain.Entities;
using MediatR;

namespace Business.Handlers.Bills.GetBillDetails;

public class GetBillDetailsHandler(
    ICachingService cachingService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetBillDetailsQuery, GetBillDetailsResponse>
{
    public async Task<GetBillDetailsResponse> Handle(
        GetBillDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var entry = await cachingService.GetAsync<ImageUploadCacheEntry>(
            CacheKeys.GetImageUrlCacheKey(request.PublicId));

        if (entry is null)
            throw new KeyNotFoundException($"No upload found for public ID '{request.PublicId}'.");

        if (entry.UserId != request.UserId)
            throw new UnauthorizedAccessException("The requested upload does not belong to the current user.");

        var bill = await unitOfWork.Repository<Bill>()
            .GetBySpecificationAsync(new GetBillByImgUrlSpecification(entry.SecureUrl));

        if (bill is { Status: BillStatus.Processed })
            return new GetBillDetailsResponse(BillStatus.Processed, entry.SecureUrl, null, BillDto.From(bill));

        var result = await cachingService.GetAsync<ImageProcessResult>(
            CacheKeys.GetProcessResultCacheKey(entry.UserId, entry.SecureUrl));

        if (result is null)
            throw new KeyNotFoundException("OCR result is not ready yet.");

        return new GetBillDetailsResponse(BillStatus.Unprocessed, entry.SecureUrl, result, null);
    }
}
