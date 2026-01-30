using BillScanner.Controllers.Base;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Business.Handlers.Images.GetUploadStorageSignature.Dto;

namespace BillScanner.Controllers
{
    public class FileStorageController(IMediator mediator) : BaseApiController
    {
        [HttpGet("signature")]
        public async Task<IActionResult> GetUploadStorageSignature([FromQuery] bool isInvoice)
        {
            var result = await mediator.Send(new GetUploadStorageSignatureRequest(isInvoice));
            return Ok(result);
        }
    }
}