using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.DTOs.Requests;
using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Domain.Authorization;
using Maliev.Aspire.ServiceDefaults.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Maliev.LeaveService.Api.Security;

namespace Maliev.LeaveService.Api.Controllers;

/// <summary>
/// Controller for managing leave requests.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("leave/v{version:apiVersion}/[controller]")]
public class LeaveRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaveRequestsController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public LeaveRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Submits a new leave request.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="dto">The leave request data.</param>
    /// <returns>The created leave request.</returns>
    [HttpPost("{employeeId:guid}")]
    [RequirePermission(LeavePermissions.Create)]
    public async Task<IActionResult> Submit(Guid employeeId, [FromBody] SubmitLeaveRequestDto dto)
    {
        if (!LeaveUserAccess.CanActForEmployee(User, employeeId))
        {
            return Forbid();
        }

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

    /// <summary>
    /// Gets leave requests for an employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="year">Optional year filter.</param>
    /// <returns>List of leave requests.</returns>
    [HttpGet("employee/{employeeId:guid}")]
    [RequirePermission(LeavePermissions.Read)]
    public async Task<IActionResult> GetByEmployee(Guid employeeId, [FromQuery] int? year)
    {
        if (!LeaveUserAccess.CanActForEmployee(User, employeeId))
        {
            return Forbid();
        }

        var query = new GetLeaveRequestsQuery
        {
            EmployeeId = employeeId,
            Year = year
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets pending leave approvals for a manager.
    /// </summary>
    /// <param name="managerId">The manager identifier.</param>
    /// <returns>List of pending leave approvals.</returns>
    [HttpGet("pending/{managerId:guid}")]
    [RequirePermission(LeavePermissions.Read)]
    public async Task<IActionResult> GetPendingApprovals(Guid managerId)
    {
        if (!LeaveUserAccess.CanActForEmployee(User, managerId))
        {
            return Forbid();
        }

        var query = new GetPendingApprovalsQuery { ApproverId = managerId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets the count of pending leave approvals for a manager.
    /// </summary>
    /// <param name="managerId">The manager identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of pending approvals.</returns>
    [HttpGet("pending-count")]
    [RequirePermission(LeavePermissions.Read)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingApprovalsCount([FromQuery] Guid managerId, CancellationToken cancellationToken)
    {
        if (managerId == Guid.Empty)
        {
            return Ok(new { count = 0 });
        }

        if (!LeaveUserAccess.CanActForEmployee(User, managerId))
        {
            return Forbid();
        }

        var count = await _mediator.Send(new GetPendingApprovalsCountQuery { ApproverId = managerId }, cancellationToken);
        return Ok(new { count });
    }

    /// <summary>
    /// Approves or rejects a leave request.
    /// </summary>
    /// <param name="requestId">The leave request identifier.</param>
    /// <param name="approverId">The approver identifier.</param>
    /// <param name="dto">The decision data.</param>
    /// <returns>The approval result.</returns>
    [HttpPost("{requestId:guid}/decision")]
    [RequirePermission(LeavePermissions.Approve)]
    public async Task<IActionResult> ProcessDecision(Guid requestId, [FromQuery] Guid approverId, [FromBody] ApproveRejectLeaveDto dto)
    {
        if (!LeaveUserAccess.CanActForEmployee(User, approverId))
        {
            return Forbid();
        }

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

    /// <summary>
    /// Cancels a leave request.
    /// </summary>
    /// <param name="requestId">The leave request identifier.</param>
    /// <param name="requestedBy">The person cancelling the request.</param>
    /// <param name="comments">Optional cancellation comments.</param>
    /// <returns>The cancellation result.</returns>
    [HttpPut("{requestId:guid}/cancel")]
    [RequirePermission(LeavePermissions.Cancel)]
    public async Task<IActionResult> Cancel(Guid requestId, [FromQuery] Guid requestedBy, [FromQuery] string? comments)
    {
        if (!LeaveUserAccess.CanActForEmployee(User, requestedBy))
        {
            return Forbid();
        }

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
