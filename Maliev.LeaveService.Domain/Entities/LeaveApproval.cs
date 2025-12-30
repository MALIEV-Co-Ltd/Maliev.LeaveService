using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Domain.Entities;

public class LeaveApproval
{
    public Guid Id { get; set; }
    public Guid LeaveRequestId { get; set; }
    public LeaveRequest LeaveRequest { get; set; } = null!;
    public Guid ApproverId { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? Comments { get; set; }
    public DateTimeOffset DecidedAt { get; set; }
}