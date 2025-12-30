namespace Maliev.LeaveService.Domain.Events.Published;

public class LeaveRequestApprovedEvent
{
    public Guid RequestId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTimeOffset ApprovedAt { get; set; }
}