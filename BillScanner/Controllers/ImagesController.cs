using BillScanner.Controllers.Base;
using Business.Handlers.Images.ProcessImage.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Controllers
{
    public class ImagesController(IMediator mediator) : BaseApiController
    {
        [HttpPost("process-image")]
        public async Task<IActionResult> ProcessImage([FromBody] ProcessImageCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }
    }
}