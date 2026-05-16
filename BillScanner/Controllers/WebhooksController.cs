using BillScanner.Controllers.Base;
using BillScanner.Models;
using Business.Handlers.Webhooks.FileUploadedWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Controllers
{
  public class WebhooksController(IMediator mediator) : BaseApiController
  {
    [HttpPost("cloudinary")]
    public async Task CloudinaryWebhook(
      [FromBody] CloudinaryNotification notification)
    {
      if (notification.NotificationType != "upload") return;
      if (notification.PublicId is null
          || notification.SecureUrl is null
          || notification.Context?.CustomFields.UserId is null)
        return;

      await mediator.Send(new FileUploadedWebhookCommand
      {
        PublicId = notification.PublicId,
        Url = notification.SecureUrl,
        UserId = notification.Context.CustomFields.UserId,
        IsInvoice = bool.TryParse(notification.Context?.CustomFields.IsInvoice, out var inv) && inv,
      });
    }
  }
}