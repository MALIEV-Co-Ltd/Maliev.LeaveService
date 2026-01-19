namespace Maliev.LeaveService.Application.DTOs.Requests;

public class UpdateLeavePolicyDto
{
    public decimal? DefaultEntitlement { get; set; }
    public decimal? AccrualRate { get; set; }
    public decimal? MaxCarryForward { get; set; }
    public int? RequiredApprovalLevels { get; set; }
    public int? MaxConsecutiveDays { get; set; }
    public bool? IsActive { get; set; }
}