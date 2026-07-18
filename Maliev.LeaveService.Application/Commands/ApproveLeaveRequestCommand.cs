using MediatR;

namespace Maliev.LeaveService.Application.Commands;

/// <summary>
/// Command to approve a pending leave request.
/// </summary>
public class ApproveLeaveRequestCommand : IRequest<CommandResult>
{
    /// <summary>
    /// Gets or sets the unique identifier of the leave request to approve.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the manager approving the request.
    /// </summary>
    public Guid ApproverId { get; set; }

    /// <summary>
    /// Gets or sets optional comments from the approver.
    /// </summary>
    public string? Comments { get; set; }
}
