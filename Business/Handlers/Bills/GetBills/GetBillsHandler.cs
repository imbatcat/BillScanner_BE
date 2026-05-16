using Business.Common;
using Business.Handlers.Bills.GetBills.Dto;
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
                var dataSpec = new GetBillsSpecification(request.UserId, request.Params, applyPaging: true);
                var countSpec = new GetBillsSpecification(request.UserId, request.Params, applyPaging: false);

                var bills = await unitOfWork.Repository<Bill>().GetAllWithSpecificationAsync(dataSpec);
                var total = await unitOfWork.Repository<Bill>().CountAsync(countSpec);

                var dtos = bills.Select(b => new BillDto
                {
                    Id = b.Id,
                    BillDate = b.BillDate,
                    BillTime = b.BillTime,
                    MerchantName = b.MerchantName,
                    Currency = b.Currency,
                    Total = b.Total,
                    ImgUrl = b.ImgUrl,
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
            var prefix = $"result:{userId}:";
            var keys = await cachingService.GetKeysByPatternAsync($"{prefix}*");

            var tasks = keys.Select(async key =>
            {
                var cached = await cachingService.GetAsync<CachedOcrResult>(key);
                if (cached is null) return null;

                Guid.TryParse(key[prefix.Length..], out var stableId);

                return new BillDto
                {
                    Id = stableId,
                    BillDate = cached.Data.BillDate.Value ?? DateOnly.MinValue,
                    BillTime = cached.Data.BillTime.Value,
                    MerchantName = cached.Data.Vendor.Name.Value,
                    Currency = cached.Data.Currency.Value ?? "VND",
                    Total = cached.Data.Total.Value,
                    ImgUrl = cached.ImageUrl,
                    ExtractionMethod = ExtractionMethod.Ocr,
                };
            });

            return [..(await Task.WhenAll(tasks)).OfType<BillDto>()];
        }
    }
}
