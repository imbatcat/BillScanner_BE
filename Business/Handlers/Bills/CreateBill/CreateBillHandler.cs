using Business.Common;
using Business.Handlers.Bills.CreateBill.Dto;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Business.Interfaces.Repositories;
using Business.Interfaces.Builders;
using Business.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Business.Handlers.Bills.CreateBill;

public class CreateBillHandler(
    IBuilderFactory builderFactory,
    ICachingService cachingService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBillCommand, CreateBillResponse>
{
    public async Task<CreateBillResponse> Handle(CreateBillCommand request, CancellationToken cancellationToken)
    {
        ImageProcessResult result = null!;
        if (request.ImgUrl != null)
        {
            result =
                await cachingService.GetAsync<ImageProcessResult>(
                    CacheKeys.GetProcessResultCacheKey(request.UserId, request.ImgUrl)) ??
                throw new InvalidOperationException("No result found for the given user and image URL.");
        }

        var billBuilder = builderFactory.Builder<IBillBuilder>()
            .FromProcessResult(result)
            .WithUserId(request.UserId)
            .WithImgUrl(request.ImgUrl)
            .WithExtractionMethod(request.ExtractionMethod)
            .WithUserEdits(request.UserEdits);

        var bill = billBuilder.Build();

        unitOfWork.Repository<Bill>().Insert(bill);

        await unitOfWork.CommitAsync();

        if (request.ImgUrl != null)
            await cachingService.RemoveAsync(CacheKeys.GetProcessResultCacheKey(request.UserId, request.ImgUrl));

        return new CreateBillResponse
        {
            BillId = bill.Id
        };
    }
}
