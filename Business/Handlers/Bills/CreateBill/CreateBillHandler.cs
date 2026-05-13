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
    IUnitOfWork unitOfWork,
    IExchangeRateService exchangeRateService) : IRequestHandler<CreateBillCommand, CreateBillResponse>
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

        if (!bill.Currency.Equals("VND", StringComparison.OrdinalIgnoreCase))
        {
            var currency = bill.Currency;
            bill.Total    = bill.Total    is { } t ? await exchangeRateService.ConvertToVndAsync(t, currency) : null;
            bill.SubTotal = bill.SubTotal is { } s ? await exchangeRateService.ConvertToVndAsync(s, currency) : null;
            bill.Tax      = bill.Tax      is { } x ? await exchangeRateService.ConvertToVndAsync(x, currency) : null;
            bill.Discount = await exchangeRateService.ConvertToVndAsync(bill.Discount, currency);
            foreach (var item in bill.BillItems)
            {
                item.UnitPrice  = await exchangeRateService.ConvertToVndAsync(item.UnitPrice,  currency);
                item.TotalPrice = await exchangeRateService.ConvertToVndAsync(item.TotalPrice, currency);
            }
            bill.Currency = "VND";
        }

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
