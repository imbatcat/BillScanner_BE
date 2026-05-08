using System.Security.Claims;
using BillScanner.Controllers.Base;
using BillScanner.Models.Bills;
using Business.Handlers.Bills.CreateBill.Dto;
using Business.Handlers.Bills.GetBillDetails;
using Business.Handlers.Bills.GetBills.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Controllers;

public class BillsController(IMediator mediator) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetBills([FromQuery] BillParams billParams)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetBillsQuery(userId, billParams));
        return Ok(ResultWithPagination(result.Items, result.Total, billParams.Page, billParams.Size));
    }

    [HttpGet("details")]
    public async Task<IActionResult> GetBillDetails([FromQuery] string publicId)
    {
        var result = await mediator.Send(new GetBillDetailsQuery(GetUserId(), publicId));
        return Ok(result);
    }

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