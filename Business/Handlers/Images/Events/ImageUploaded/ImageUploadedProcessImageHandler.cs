using Domain.Events;
using MediatR;

namespace Business.Handlers.Images.Events.ImageUploaded;

public class ImageUploadedProcessImageHandler :
  INotificationHandler<ImageUploadedEvent>
{
  public Task Handle(ImageUploadedEvent notification, CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}