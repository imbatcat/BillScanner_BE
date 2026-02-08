using Infrastructure.MarkerInterfaces;

namespace Infrastructure.Services.FileStorage.Cloudinary
{
    public class CloudinarySettings : IAppSettings
    {
        public string CloudName { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public string ApiSecret { get; set; } = null!;
        public string InvoiceFolderName { get; set; } = null!;
        public string ReceiptFolderName { get; set; } = null!;
    }
}