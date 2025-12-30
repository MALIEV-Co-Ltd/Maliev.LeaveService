using System.ComponentModel.DataAnnotations;
using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Application.DTOs.Requests;

public class SubmitLeaveRequestDto
{
    [Required]
    public LeaveType LeaveType { get; set; }

    [Required]
    public DateTimeOffset StartDate { get; set; }

    [Required]
    public DateTimeOffset EndDate { get; set; }

    public HalfDayPeriod HalfDayPeriod { get; set; } = HalfDayPeriod.FullDay;

    [MaxLength(500)]
    public string? Reason { get; set; }
}