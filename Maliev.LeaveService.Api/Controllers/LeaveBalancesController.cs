using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Domain.Authorization;
using Maliev.Aspire.ServiceDefaults.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Maliev.LeaveService.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("leave/v{version:apiVersion}/[controller]")]
public class LeaveBalancesController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaveBalancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{employeeId:guid}")]
    [RequirePermission(LeavePermissions.Read)]
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
