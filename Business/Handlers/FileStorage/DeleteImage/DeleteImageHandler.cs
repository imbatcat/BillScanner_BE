using System.Text.RegularExpressions;
using Business.Common;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Business.Specifications.Bills;
using Domain.Entities;
using MediatR;

namespace Business.Handlers.FileStorage.DeleteImage;

public class DeleteImageHandler(
    IFileStorageService fileStorageService,
    ICachingService cachingService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteImageCommand>
{
    public async Task Handle(DeleteImageCommand request, CancellationToken cancellationToken)
    {
        var publicId = ExtractPublicId(request.ImgUrl);

        await fileStorageService.DeleteImageAsync(publicId);

        var bill = await unitOfWork.Repository<Bill>()
            .GetBySpecificationAsync(new GetBillByImgUrlSpecification(request.ImgUrl));

        if (bill is not null)
        {
            unitOfWork.Repository<Bill>().Delete(bill);
            await unitOfWork.CommitAsync();
        }

        var resultId = CacheKeys.StableIdFromUrl(request.ImgUrl);
        await cachingService.RemoveAsync(CacheKeys.GetProcessResultCacheKey(request.UserId, resultId));
    }

    private static string ExtractPublicId(string imgUrl)
    {
        var afterUpload = imgUrl[(imgUrl.IndexOf("/upload/", StringComparison.Ordinal) + 8)..];
        afterUpload = Regex.Replace(afterUpload, @"^v\d+/", "");
        var dot = afterUpload.LastIndexOf('.');
        return dot >= 0 ? afterUpload[..dot] : afterUpload;
    }
}
