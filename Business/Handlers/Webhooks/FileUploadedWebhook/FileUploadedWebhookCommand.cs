using MediatR;

namespace Business.Handlers.Webhooks.FileUploadedWebhook
{
    public class FileUploadedWebhookCommand : IRequest
    {
        public string PublicId { get; set; }
        public string Url { get; set; }
        public string UserId { get; set; }
        public bool IsInvoice { get; set; }
    }
}