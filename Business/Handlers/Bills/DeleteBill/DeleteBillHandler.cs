using System.Text.RegularExpressions;
using Business.Common;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Business.Handlers.Bills.DeleteBill;

public class DeleteBillHandler(
    IFileStorageService fileStorageService,
    ICachingService cachingService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBillCommand>
{
    public async Task Handle(DeleteBillCommand request, CancellationToken cancellationToken)
    {
        var bill = await unitOfWork.Repository<Bill>().GetByIdAsync(request.Id);
        if (bill is not null)
        {
            if (bill.UserId != request.UserId)
                throw new UnauthorizedAccessException("Bill does not belong to current user.");
            if (bill.ImgUrl is not null)
                await fileStorageService.DeleteImageAsync(ExtractPublicId(bill.ImgUrl));
            unitOfWork.Repository<Bill>().Delete(bill);
            await unitOfWork.CommitAsync();
            await cachingService.RemoveAsync(CacheKeys.GetProcessResultCacheKey(request.UserId, request.Id));
        }
        else
        {
            var cached = await cachingService.GetAsync<CachedOcrResult>(
                CacheKeys.GetProcessResultCacheKey(request.UserId, request.Id));
            if (cached is null)
                throw new KeyNotFoundException("Bill not found.");
            await fileStorageService.DeleteImageAsync(ExtractPublicId(cached.ImageUrl));
            await cachingService.RemoveAsync(CacheKeys.GetProcessResultCacheKey(request.UserId, request.Id));
        }
    }

    private static string ExtractPublicId(string imgUrl)
    {
        var afterUpload = imgUrl[(imgUrl.IndexOf("/upload/", StringComparison.Ordinal) + 8)..];
        afterUpload = Regex.Replace(afterUpload, @"^v\d+/", "");
        var dot = afterUpload.LastIndexOf('.');
        return dot >= 0 ? afterUpload[..dot] : afterUpload;
    }
}
