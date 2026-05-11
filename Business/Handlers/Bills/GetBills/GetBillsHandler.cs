using System.Security.Cryptography;
using System.Text;
using Business.Common;
using Business.Handlers.Bills.GetBills.Dto;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Business.Specifications.Bills;
using Domain.Entities;
using MediatR;

namespace Business.Handlers.Bills.GetBills
{
    public class GetBillsHandler(IUnitOfWork unitOfWork, ICachingService cachingService)
        : IRequestHandler<GetBillsQuery, GetBillsResponse>
    {
        public async Task<GetBillsResponse> Handle(GetBillsQuery request, CancellationToken cancellationToken)
        {
            if (request.Params.IsProcessed)
            {
                var dataSpec  = new GetBillsSpecification(request.UserId, request.Params, applyPaging: true);
                var countSpec = new GetBillsSpecification(request.UserId, request.Params, applyPaging: false);

                var bills = await unitOfWork.Repository<Bill>().GetAllWithSpecificationAsync(dataSpec);
                var total = await unitOfWork.Repository<Bill>().CountAsync(countSpec);

                var dtos = bills.Select(b => new BillDto
                {
                    Id               = b.Id,
                    BillDate         = b.BillDate,
                    BillTime         = b.BillTime,
                    MerchantName     = b.MerchantName,
                    Currency         = b.Currency,
                    Total            = b.Total,
                    ImgUrl           = b.ImgUrl,
                    ExtractionMethod = b.ExtractionMethod,
                }).ToList();

                return new GetBillsResponse(dtos, total);
            }
            else
            {
                var cachedDtos = await GetCachedBillDtosAsync(request.UserId);
                return new GetBillsResponse(cachedDtos, cachedDtos.Count);
            }
        }

        private async Task<List<BillDto>> GetCachedBillDtosAsync(Guid userId)
        {
            var prefix = CacheKeys.GetProcessResultCacheKey(userId, string.Empty);
            var keys = await cachingService.GetKeysByPatternAsync($"{prefix}*");

            var tasks = keys.Select(async key =>
            {
                var imgUrl = key[prefix.Length..];
                var result = await cachingService.GetAsync<ImageProcessResult>(key);
                if (result is null) return null;

                var stableId = StableIdFromImgUrl(imgUrl);
                await cachingService.SetAsync(
                    CacheKeys.GetBillRefCacheKey(userId, stableId),
                    imgUrl,
                    TimeSpan.FromMinutes(10));

                return new BillDto
                {
                    Id               = stableId,
                    BillDate         = result.BillDate.Value ?? DateOnly.MinValue,
                    BillTime         = result.BillTime.Value,
                    MerchantName     = result.Vendor.Name.Value,
                    Currency         = result.Currency.Value ?? "VND",
                    Total            = result.Total.Value,
                    ImgUrl           = imgUrl,
                    ExtractionMethod = ExtractionMethod.Ocr,
                };
            });

            return [..(await Task.WhenAll(tasks)).OfType<BillDto>()];
        }

        private static Guid StableIdFromImgUrl(string imgUrl)
        {
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(imgUrl));
            return new Guid(hash);
        }
    }
}
