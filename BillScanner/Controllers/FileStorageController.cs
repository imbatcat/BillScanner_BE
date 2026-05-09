using BillScanner.Controllers.Base;
using Business.Handlers.FileStorage.DeleteImage;
using Business.Handlers.Images.GetUploadStorageSignature.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Controllers
{
    public class FileStorageController(IMediator mediator) : BaseApiController
    {
        [HttpGet("signature")]
        public async Task<ActionResult<GetUploadStorageSignatureResponse>> GetUploadStorageSignature(
            [FromQuery] bool isInvoice)
        {
            var userId = GetUserId();
            var result = await mediator.Send(new GetUploadStorageSignatureRequest(isInvoice, userId));
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteImage([FromQuery] string imgUrl)
        {
            await mediator.Send(new DeleteImageCommand(GetUserId(), imgUrl));
            return NoContent();
        }
    }
}