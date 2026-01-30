using MediatR;

namespace Domain.Events
{
    public record ImageUploadedEvent(string Url) : INotification;
}