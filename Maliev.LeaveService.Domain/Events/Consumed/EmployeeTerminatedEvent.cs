namespace Maliev.LeaveService.Domain.Events.Consumed;

public class EmployeeTerminatedEvent
{
    public Guid EmployeeId { get; set; }
    public DateTimeOffset TerminationDate { get; set; }
    public string? TerminationReason { get; set; }
}