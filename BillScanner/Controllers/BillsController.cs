using System.Security.Claims;
using BillScanner.Controllers.Base;
using BillScanner.Models.Bills;
using Business.Handlers.Bills.CreateBill.Dto;
using Business.Handlers.Bills.GetBillDetails;
using Business.Handlers.Bills.GetBills.Dto;
using Business.Handlers.Bills.UpdateBill;
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
    public async Task<IActionResult> GetBillDetails([FromQuery] Guid id)
    {
        var result = await mediator.Send(new GetBillDetailsQuery(GetUserId(), id));
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBill(Guid id, [FromBody] UpdateBillModel model)
    {
        await mediator.Send(new UpdateBillCommand
        {
            UserId    = GetUserId(),
            Id        = id,
            UserEdits = model.UserEdits,
        });
        return NoContent();
    }
}