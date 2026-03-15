using BillScanner.Controllers.Base;
using BillScanner.Models.Bills;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillScanner.Controllers;

public class BillsController(IMediator mediator) : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateBill([FromBody] CreateBillModel model)
    {
        var result = await mediator.Send(model);
        return Ok(result);
    }
}