using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Domain.Authorization;
using Maliev.Aspire.ServiceDefaults.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Maliev.LeaveService.Api.Controllers;

/// <summary>
/// Provides leave-related reports and analytics.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("leave/v{version:apiVersion}/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance for decoupled query handling.</param>
    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get organizational leave utilization report.
    /// </summary>
    /// <param name="year">The year for which to generate the report (defaults to current year).</param>
    /// <param name="departmentId">Optional department filter.</param>
    /// <returns>Leave utilization statistics.</returns>
    [HttpGet("utilization")]
    [RequirePermission(LeavePermissions.Reports)]
    public async Task<IActionResult> GetUtilizationReport([FromQuery] int? year, [FromQuery] Guid? departmentId)
    {
        var query = new GetLeaveUtilizationReportQuery
        {
            Year = year ?? DateTime.UtcNow.Year,
            DepartmentId = departmentId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
