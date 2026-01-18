using Maliev.LeaveService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Maliev.LeaveService.Application.DTOs.Requests;

public class CreateLeavePolicyDto
{
    [Required]
    public LeaveType LeaveType { get; set; }

    [Required]
    [Range(0, 365)]
    public decimal DefaultEntitlement { get; set; }


    public decimal AccrualRate { get; set; }

    public decimal MaxCarryForward { get; set; }

    [Required]
    public int RequiredApprovalLevels { get; set; }

    public int MaxConsecutiveDays { get; set; }
}