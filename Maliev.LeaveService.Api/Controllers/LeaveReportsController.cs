using Maliev.LeaveService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.LeaveService.Api.Controllers;

[ApiController]
[Route("leave/v1/[controller]")]
[Authorize]
public class LeaveReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaveReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("utilization")]
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
