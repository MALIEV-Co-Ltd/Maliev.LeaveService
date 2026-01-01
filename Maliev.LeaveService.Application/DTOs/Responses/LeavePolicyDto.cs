using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Application.DTOs.Responses;

public class LeavePolicyDto
{
    public Guid Id { get; set; }
    public LeaveType LeaveType { get; set; }
    public decimal DefaultEntitlement { get; set; }
    public bool RequiresApproval { get; set; }
    public int MaxCarryOverDays { get; set; }
    public int MaxConsecutiveDays { get; set; }
    public bool IsActive { get; set; }
}
