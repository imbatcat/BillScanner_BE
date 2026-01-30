using Business.Interfaces.Repositories;
using Domain.Events;
using MediatR;

namespace Business.Handlers.Webhooks.FileUploadedWebhook
{
    public class FileUploadedWebhookHandler(
        IPublisher _publisher) : IRequestHandler<FileUploadedWebhookCommand>
    {
        public async Task Handle(FileUploadedWebhookCommand request, CancellationToken cancellationToken)
        {
            // if add any table(s) related to file upload tracking, add them here
            await _publisher.Publish(new ImageUploadedEvent(request.Url), cancellationToken);
        }
    }
}