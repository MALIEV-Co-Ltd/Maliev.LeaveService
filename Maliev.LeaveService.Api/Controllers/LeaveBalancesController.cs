using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Domain.Authorization;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.LeaveService.Api.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Maliev.LeaveService.Api.Controllers;

/// <summary>
/// Controller for managing employee leave balances.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("leave/v{version:apiVersion}/[controller]")]
public class LeaveBalancesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaveBalancesController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public LeaveBalancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets leave balances for an employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="year">Optional year filter.</param>
    /// <returns>Leave balance information.</returns>
    [HttpGet("{employeeId:guid}")]
    [RequirePermission(LeavePermissions.Read)]
    public async Task<IActionResult> GetBalances(Guid employeeId, [FromQuery] int? year)
    {
        if (!LeaveUserAccess.CanActForEmployee(User, employeeId))
        {
            return Forbid();
        }

        var query = new GetLeaveBalancesQuery
        {
            EmployeeId = employeeId,
            Year = year
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
