namespace Maliev.LeaveService.Domain.Events.Published;

public class LeaveRequestCancelledEvent
{
    public Guid RequestId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CancelledAt { get; set; }
}