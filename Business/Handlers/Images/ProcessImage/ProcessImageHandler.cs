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
                request.Url,
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
            var missingFields = new List<string>();
            if (result.Vendor.Name.Value == null)
            {
                missingFields.Add("MerchantName");
            }

            if (result.BillDate.Value == null)
            {
                missingFields.Add("TransactionDate");
            }

            if (result.Items.Count == 0)
            {
                missingFields.Add("Items");
            }

            if (result.Items.Any(item => item.ItemName.Value == null))
            {
                missingFields.Add("ItemName");
            }

            if (result.Items.Any(item => item.TotalPrice.Value == null))
            {
                missingFields.Add("ItemTotalPrice");
            }

            if (result.Total.Value == null)
            {
                missingFields.Add("Total");
            }

            if (missingFields.Count > settings.Value.MinimumMissingFields)
            {
                throw new ScanRetryRequiredException();
            }

            return missingFields;
        }
    }
}
            