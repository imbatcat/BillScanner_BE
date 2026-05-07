using BillScanner.Controllers.Base;
using BillScanner.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Controllers
{
  public class WebhooksControllers(IMediator mediator) : BaseApiController
  {
    [HttpPost("cloudinary")]
    public async Task<IActionResult> CloudinaryWebhook(
      [FromBody] CloudinaryNotification notification)
    {
      switch (notification.NotificationType)
      {
        case "upload":
          Console.WriteLine($"Upload completed: {notification.PublicId}");
          break;
        case "delete_by_token":
          Console.WriteLine($"Delete by token completed: {notification.PublicId}");
          break;
      }

      return Ok();
    }
  }
}