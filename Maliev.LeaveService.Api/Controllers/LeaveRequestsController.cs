using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.DTOs.Requests;
using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.LeaveService.Api.Controllers;

[ApiController]
[Route("leave/v1/[controller]")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaveRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{employeeId:guid}")]
    public async Task<IActionResult> Submit(Guid employeeId, [FromBody] SubmitLeaveRequestDto dto)
    {
        var command = new SubmitLeaveRequestCommand
        {
            EmployeeId = employeeId,
            LeaveType = dto.LeaveType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            HalfDayPeriod = dto.HalfDayPeriod,
            Reason = dto.Reason
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetByEmployee), new { employeeId }, result);
        }

        return BadRequest(new { message = result.ErrorMessage });
    }

    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId, [FromQuery] int? year)
    {
        var query = new GetLeaveRequestsQuery
        {
            EmployeeId = employeeId,
            Year = year
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("pending/{managerId:guid}")]
    public async Task<IActionResult> GetPendingApprovals(Guid managerId)
    {
        var query = new GetPendingApprovalsQuery { ApproverId = managerId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{requestId:guid}/decision")]
    public async Task<IActionResult> ProcessDecision(Guid requestId, [FromQuery] Guid approverId, [FromBody] ApproveRejectLeaveDto dto)
    {
        var command = new ApproveRejectLeaveCommand
        {
            RequestId = requestId,
            ApproverId = approverId,
            Decision = dto.Decision,
            Comments = dto.Comments
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(new { message = result.ErrorMessage });
    }

    [HttpPut("{requestId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid requestId, [FromQuery] Guid requestedBy, [FromQuery] string? comments)
    {
        var command = new CancelLeaveRequestCommand
        {
            RequestId = requestId,
            RequestedBy = requestedBy,
            Comments = comments
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(new { message = result.ErrorMessage });
    }
}