using System.Security.Claims;
using BillScanner.Controllers.Base;
using BillScanner.Models.Bills;
using Business.Handlers.Bills.CreateBill.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Controllers;

public class BillsController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateBill([FromBody] CreateBillModel model)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return UnauthorizedWithMessage("User not authenticated");
        }

        var command = new CreateBillCommand
        {
            UserId = Guid.Parse(userIdClaim),
            ImgUrl = model.ImgUrl,
            ExtractionMethod = model.ExtractionMethod,
            UserEdits = model.UserEdits
        };

        var result = await mediator.Send(command);
        return Ok(result);
    }
}