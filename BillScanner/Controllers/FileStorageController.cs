using Business.Handlers.Images.GetUploadStorageSignature;
using BillScanner.Controllers.Base;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Controllers;

public class FileStorageController(IMediator mediator) : BaseApiController
{
    [HttpGet("signature")]
    public async Task<IActionResult> GetUploadStorageSignature()
    {
        var result = await mediator.Send(new GetUploadStorageSignatureRequest());
        return Ok(result);
    }
}