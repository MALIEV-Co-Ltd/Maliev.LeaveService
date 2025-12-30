using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Application.DTOs.Responses;

public class LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public HalfDayPeriod HalfDayPeriod { get; set; }
    public string? Reason { get; set; }
    public LeaveRequestStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}