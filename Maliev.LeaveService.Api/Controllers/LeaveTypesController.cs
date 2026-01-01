using Maliev.LeaveService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.LeaveService.Api.Controllers;

/// <summary>
/// Manages available leave types and their associated policies.
/// </summary>
[ApiController]
[Route("leave/v1/[controller]")]
[Authorize]
public class LeaveTypesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaveTypesController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance for decoupled query handling.</param>
    public LeaveTypesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all available leave types and their policies.
    /// </summary>
    /// <returns>A collection of leave policies.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetLeaveTypesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
