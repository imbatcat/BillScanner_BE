using Business.Common;
using Business.Handlers.Images.ProcessImage.Dto;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Business.Interfaces.Services;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Options;

namespace Business.Handlers.Images.ProcessImage
{
    public class ProcessImageHandler(
        IOptions<BusinessSettings> settings,
        ICachingService cachingService,
        IImageExtractionService imageProcessingService) :
        IRequestHandler<ProcessImageCommand, ProcessImageResponse>
    {
        public async Task<ProcessImageResponse> Handle(ProcessImageCommand request, CancellationToken cancellationToken)
        {
            var result = await imageProcessingService.ExtractImageAsync(
                request.Url,
                request.IsInvoice
            );

            var missingFields = ValidateMissingFields(result);
            ValidateTotalPrice(result);

            await cachingService.SetAsync(
                CacheKeys.GetProcessResultCacheKey(request.UserId, request.Url),
                result,
                TimeSpan.FromMinutes(settings.Value.CacheExpirationTimeInMinutes));

            return new ProcessImageResponse(result, missingFields);
        }

        // private static void ValidateInvoiceDate(ImageProcessResult result, ProcessImageCommand request)
        // {
        //     if (request.IsInvoice && result.TransactionDate.Value < DateTime.Now)
        //     {
        //         throw new ScanRetryRequiredException();
        //     }
        // }

        private static void ValidateTotalPrice(ImageProcessResult result)
        {
            if (result.SubTotal.Value == null)
            {
                var calculatedTotal = result.Items.Sum(item => item.TotalPrice.Value);
                var expectedTotal = result.Total.Value;
                if (calculatedTotal != expectedTotal)
                {
                    throw new ScanRetryRequiredException();
                }
            }
            else
            {
                var calculatedTotal = result.Items.Sum(item => item.TotalPrice.Value);
                var receiptTotal = result.Total.Value;
                var expectedTotal = result.SubTotal.Value + result.Tax.Value;

                if (calculatedTotal != expectedTotal || calculatedTotal != receiptTotal)
                {
                    throw new ScanRetryRequiredException();
                }
            }
        }

        private List<string> ValidateMissingFields(ImageProcessResult result)
        {
            var checks = new (Func<bool> IsMissing, string FieldName)[]
            {
                (() => result.Vendor.Name.Value == null, "MerchantName"),
                (() => result.BillDate.Value == null, "TransactionDate"),
                (() => result.Items.Count == 0, "Items"),
                (() => result.Items.Any(i => i.ItemName.Value == null), "ItemName"),
                (() => result.Items.Any(i => i.Quantity.Value == null), "Quantity"),
                (() => result.Items.Any(i => i.UnitPrice.Value == null), "UnitPrice"),
                (() => result.Items.Any(i => i.TotalPrice.Value == null), "ItemTotalPrice"),
                (() => result.Total.Value == null, "Total"),
            };

            var missingFields = checks
                .Where(c => c.IsMissing())
                .Select(c => c.FieldName)
                .ToList();

            return missingFields.Count > settings.Value.MinimumMissingFields
                ? throw new ScanRetryRequiredException()
                : missingFields;
        }
    }
}
            