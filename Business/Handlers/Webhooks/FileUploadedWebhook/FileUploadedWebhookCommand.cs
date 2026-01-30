using MediatR;

namespace Business.Handlers.Webhooks.FileUploadedWebhook
{
    public class FileUploadedWebhookCommand : IRequest
    {
      public string StorageProvider { get; set; }
      public string PublicId { get; set; }
      public string Url { get; set; }
      public long SizeBytes { get; set; }
      public string MimeType { get; set; }
    }
}