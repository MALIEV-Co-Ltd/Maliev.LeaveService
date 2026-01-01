using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Domain.Events.Published;

public class LeaveRequestSubmittedEvent
{
    public Guid RequestId { get; set; }
    public Guid EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
}