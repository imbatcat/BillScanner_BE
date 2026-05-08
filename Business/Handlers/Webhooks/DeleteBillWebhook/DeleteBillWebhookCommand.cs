using MediatR;

namespace Business.Handlers.Webhooks.DeleteBillWebhook
{
    public class DeleteBillWebhookCommand : IRequest
    {
        public string PublicId { get; set; }
    }
}
