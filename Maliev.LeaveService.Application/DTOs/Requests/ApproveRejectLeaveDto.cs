using Maliev.LeaveService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Maliev.LeaveService.Application.DTOs.Requests;

public class ApproveRejectLeaveDto
{
    [Required]
    public ApprovalStatus Decision { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }
}
