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
public class LeaveReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaveReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

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
