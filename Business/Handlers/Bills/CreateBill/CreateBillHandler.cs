using System.Security.Cryptography;
using System.Text;
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
        var billBuilder = builderFactory.Builder<IBillBuilder>()
            .WithUserId(request.UserId)
            .WithImgUrl(request.ImgUrl)
            .WithExtractionMethod(request.ExtractionMethod);

        if (request.ExtractionMethod == ExtractionMethod.Ocr)
        {
            if (request.ImgUrl == null)
                throw new InvalidOperationException("ImgUrl is required for OCR extraction.");

            var result = await cachingService.GetAsync<ImageProcessResult>(
                CacheKeys.GetProcessResultCacheKey(request.UserId, request.ImgUrl))
                ?? throw new InvalidOperationException("No cached OCR result found.");

            billBuilder.FromProcessResult(result);
        }

        billBuilder.WithUserEdits(request.UserEdits);

        var bill = billBuilder.Build();

        unitOfWork.Repository<Bill>().Insert(bill);

        await unitOfWork.CommitAsync();

        if (request.ImgUrl != null)
        {
            await cachingService.RemoveAsync(CacheKeys.GetProcessResultCacheKey(request.UserId, request.ImgUrl));
            var stableId = new Guid(MD5.HashData(Encoding.UTF8.GetBytes(request.ImgUrl)));
            await cachingService.RemoveAsync(CacheKeys.GetBillRefCacheKey(request.UserId, stableId));
        }

        return new CreateBillResponse
        {
            BillId = bill.Id
        };
    }
}
