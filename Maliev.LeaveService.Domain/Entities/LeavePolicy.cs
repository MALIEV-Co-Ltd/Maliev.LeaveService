using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Domain.Entities;

public class LeavePolicy
{
    public Guid Id { get; set; }
    public LeaveType LeaveType { get; set; }
    public decimal DefaultEntitlement { get; set; }
    public decimal AccrualRate { get; set; }
    public decimal MaxCarryForward { get; set; }
    public int MaxConsecutiveDays { get; set; }
    public int RequiredApprovalLevels { get; set; }
    public bool IsActive { get; set; }
}