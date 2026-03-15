using BillScanner.Controllers.Base;
using BillScanner.Models.Images;
using Business.Handlers.Images.ProcessImage.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Controllers
{
    public class ImagesController(IMediator mediator) : BaseApiController
    {
        [HttpPost("process")]
        public async Task<IActionResult> ProcessImage([FromBody] ProcessImageModel model)
        {
            var command = new ProcessImageCommand
            {
                Url = model.Url,
                IsInvoice = model.IsInvoice,
                UserId = GetUserId()
            };

            var result = await mediator.Send(command);
            return Ok(result);
        }

        // [HttpPost("retry")] // TODO: complete this 
        // public async Task<IActionResult> RetryImage([FromBody] RetryImageCommand command)
        // {
        //     var result = await mediator.Send(command);
        //     return Ok(result);
        // }
    }
}