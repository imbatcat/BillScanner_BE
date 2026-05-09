using Business.Common;
using Business.Handlers.Bills.GetBillDetails.Dto;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
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
        var bill = await unitOfWork.Repository<Bill>()
            .GetBySpecificationAsync(new GetBillByImgUrlSpecification(request.ImgUrl));

        if (bill is not null)
        {
            if (bill.UserId != request.UserId)
                throw new UnauthorizedAccessException("The requested bill does not belong to the current user.");

            return new GetBillDetailsResponse(request.ImgUrl, null, BillDto.From(bill));
        }

        var result = await cachingService.GetAsync<ImageProcessResult>(
            CacheKeys.GetProcessResultCacheKey(request.UserId, request.ImgUrl));

        if (result is null)
            throw new KeyNotFoundException("Bill not found.");

        return new GetBillDetailsResponse(request.ImgUrl, result, null);
    }
}
