using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Application.DTOs.Responses;

public class LeaveApprovalDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid ApproverId { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? Comments { get; set; }
    public DateTimeOffset DecidedAt { get; set; }
}