using System.ComponentModel.DataAnnotations;
using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Application.DTOs.Requests;

public class CreateLeavePolicyDto
{
    [Required]
    public LeaveType LeaveType { get; set; }

    [Required]
    public decimal DefaultEntitlement { get; set; }

    public decimal AccrualRate { get; set; }

    public decimal MaxCarryForward { get; set; }

    [Required]
    public int RequiredApprovalLevels { get; set; }

    public int MaxConsecutiveDays { get; set; }
}