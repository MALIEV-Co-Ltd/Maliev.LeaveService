namespace Maliev.LeaveService.Domain.Events.Published;

public class LeaveRequestRejectedEvent
{
    public Guid RequestId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset RejectedAt { get; set; }
}