using MediatR;

namespace Maliev.LeaveService.Application.Commands;

/// <summary>
/// Command to cancel a pending or approved leave request.
/// </summary>
public class CancelLeaveRequestCommand : IRequest<CommandResult>
{
    /// <summary>
    /// Gets or sets the unique identifier of the leave request to cancel.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the employee or manager cancelling the request.
    /// </summary>
    public Guid RequestedBy { get; set; }

    /// <summary>
    /// Gets or sets optional comments for cancellation.
    /// </summary>
    public string? Comments { get; set; }
}
