using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Application.Interfaces;
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
    private readonly IEmployeeServiceClient _employeeServiceClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaveBalancesController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="employeeServiceClient">The employee service client used for principal-to-employee access checks.</param>
    public LeaveBalancesController(IMediator mediator, IEmployeeServiceClient employeeServiceClient)
    {
        _mediator = mediator;
        _employeeServiceClient = employeeServiceClient;
    }

    /// <summary>
    /// Gets leave balances for an employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="year">Optional year filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Leave balance information.</returns>
    [HttpGet("{employeeId:guid}")]
    [RequirePermission(LeavePermissions.BalanceRead)]
    public async Task<IActionResult> GetBalances(
        Guid employeeId,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        if (!await LeaveUserAccess.CanActForEmployeeAsync(User, employeeId, _employeeServiceClient, cancellationToken))
        {
            return Forbid();
        }

        var query = new GetLeaveBalancesQuery
        {
            EmployeeId = employeeId,
            Year = year
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
