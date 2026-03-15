using Business.Common;
using Business.Handlers.Bills.CreateBill.Dto;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Business.Interfaces.Services;
using MediatR;

namespace Business.Handlers.Bills.CreateBill;

public class CreateBillHandler(
    ICachingService cachingService) : IRequestHandler<CreateBillCommand, CreateBillResponse>
{
    public async Task<CreateBillResponse> Handle(CreateBillCommand request, CancellationToken cancellationToken)
    {
        var result =
            await cachingService.GetAsync<ImageProcessResult>(
                CacheKeys.GetProcessResultCacheKey(request.UserId, request.ImgUrl)) ??
            throw new InvalidOperationException("No result found for the given user and image URL.");
        return new CreateBillResponse
        {
            BillId = null
        };
    }
}
