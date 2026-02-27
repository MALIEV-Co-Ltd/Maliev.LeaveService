using Maliev.LeaveService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.LeaveService.Api.Controllers;

[ApiController]
[Route("leave/v1/[controller]")]
[Authorize]
public class LeaveBalancesController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaveBalancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{employeeId:guid}")]
    public async Task<IActionResult> GetBalances(Guid employeeId, [FromQuery] int? year)
    {
        var query = new GetLeaveBalancesQuery
        {
            EmployeeId = employeeId,
            Year = year
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
