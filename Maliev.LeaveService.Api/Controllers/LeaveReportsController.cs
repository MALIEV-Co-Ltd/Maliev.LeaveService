using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Domain.Authorization;
using Maliev.Aspire.ServiceDefaults.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Maliev.LeaveService.Api.Controllers;

/// <summary>
/// Controller for leave reporting and analytics.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("leave/v{version:apiVersion}/[controller]")]
public class LeaveReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaveReportsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public LeaveReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets leave utilization report.
    /// </summary>
    /// <param name="departmentId">Optional department filter.</param>
    /// <param name="startDate">Optional start date.</param>
    /// <param name="endDate">Optional end date.</param>
    /// <returns>Leave utilization data.</returns>
    [HttpGet("utilization")]
    [RequirePermission(LeavePermissions.Reports)]
    public async Task<IActionResult> GetUtilization([FromQuery] Guid? departmentId, [FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
    {
        var query = new GetUtilizationReportQuery
        {
            DepartmentId = departmentId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
