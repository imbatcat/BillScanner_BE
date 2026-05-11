using Business.Common;
using Business.Handlers.Bills.GetBillDetails.Dto;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
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
        var bill = await unitOfWork.Repository<Bill>().GetByIdAsync(request.Id);

        if (bill is not null)
        {
            if (bill.UserId != request.UserId)
                throw new UnauthorizedAccessException("The requested bill does not belong to the current user.");

            return new GetBillDetailsResponse(bill.ImgUrl ?? string.Empty, null, BillDto.From(bill));
        }

        var imgUrl = await cachingService.GetAsync<string>(
            CacheKeys.GetBillRefCacheKey(request.UserId, request.Id));

        if (imgUrl is null)
            throw new KeyNotFoundException("Bill not found.");

        var result = await cachingService.GetAsync<ImageProcessResult>(
            CacheKeys.GetProcessResultCacheKey(request.UserId, imgUrl));

        if (result is null)
            throw new KeyNotFoundException("Bill not found.");

        return new GetBillDetailsResponse(imgUrl, result, null);
    }
}
