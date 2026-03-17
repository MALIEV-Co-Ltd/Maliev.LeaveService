using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.DTOs.Requests;
using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Domain.Authorization;
using Maliev.Aspire.ServiceDefaults.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Maliev.LeaveService.Api.Controllers;

/// <summary>
/// Controller for managing leave policies.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("leave/v{version:apiVersion}/[controller]")]
public class LeavePoliciesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeavePoliciesController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public LeavePoliciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets all leave policies.
    /// </summary>
    /// <returns>List of leave policies.</returns>
    [HttpGet]
    [RequirePermission(LeavePermissions.Admin)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetLeavePoliciesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new leave policy.
    /// </summary>
    /// <param name="dto">The leave policy data.</param>
    /// <returns>The created leave policy.</returns>
    [HttpPost]
    [RequirePermission(LeavePermissions.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateLeavePolicyDto dto)
    {
        var command = new CreateLeavePolicyCommand
        {
            LeaveType = dto.LeaveType,
            DefaultEntitlement = dto.DefaultEntitlement,
            AccrualRate = dto.AccrualRate,
            MaxCarryForward = dto.MaxCarryForward,
            RequiredApprovalLevels = dto.RequiredApprovalLevels,
            MaxConsecutiveDays = dto.MaxConsecutiveDays
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }

        return BadRequest(new { message = result.ErrorMessage });
    }

    /// <summary>
    /// Updates an existing leave policy.
    /// </summary>
    /// <param name="id">The leave policy identifier.</param>
    /// <param name="dto">The updated leave policy data.</param>
    /// <returns>The updated leave policy.</returns>
    [HttpPut("{id:guid}")]
    [RequirePermission(LeavePermissions.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeavePolicyDto dto)
    {
        var command = new UpdateLeavePolicyCommand
        {
            Id = id,
            DefaultEntitlement = dto.DefaultEntitlement,
            AccrualRate = dto.AccrualRate,
            MaxCarryForward = dto.MaxCarryForward,
            RequiredApprovalLevels = dto.RequiredApprovalLevels,
            MaxConsecutiveDays = dto.MaxConsecutiveDays,
            IsActive = dto.IsActive
        };

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(new { message = result.ErrorMessage });
    }
}
