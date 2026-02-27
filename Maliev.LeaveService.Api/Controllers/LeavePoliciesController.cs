using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.DTOs.Requests;
using Maliev.LeaveService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.LeaveService.Api.Controllers;

[ApiController]
[Route("leave/v1/[controller]")]
[Authorize]
public class LeavePoliciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeavePoliciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetLeavePoliciesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
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

    [HttpPut("{id:guid}")]
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
