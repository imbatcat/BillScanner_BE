using Infrastructure.MarkerInterfaces;

namespace Infrastructure.Services.ImageProcessing
{
    public class AzureImageSettings : IAppSettings
    {
        public string Endpoint { get; init; }
        public string Region { get; init; }

        public string ApiKey1 { get; init; }
        public string ApiKey2 { get; init; }
        public string InvoiceModelId { get; init; }
        public string ReceiptModelId { get; init; }
    }
}