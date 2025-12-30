using MediatR;

namespace Maliev.LeaveService.Application.Commands;

/// <summary>
/// Command to reject a pending leave request.
/// </summary>
public class RejectLeaveRequestCommand : IRequest<CommandResult>
{
    /// <summary>
    /// Gets or sets the unique identifier of the leave request to reject.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the manager rejecting the request.
    /// </summary>
    public Guid ApproverId { get; set; }

    /// <summary>
    /// Gets or sets the mandatory comments for rejection.
    /// </summary>
    public string Comments { get; set; } = string.Empty;
}
