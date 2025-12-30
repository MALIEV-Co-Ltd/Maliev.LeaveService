using Maliev.LeaveService.Domain.Enums;
using MediatR;

namespace Maliev.LeaveService.Application.Commands;

public class ApproveRejectLeaveCommand : IRequest<CommandResult>
{
    public Guid RequestId { get; set; }
    public Guid ApproverId { get; set; }
    public ApprovalStatus Decision { get; set; }
    public string? Comments { get; set; }
}