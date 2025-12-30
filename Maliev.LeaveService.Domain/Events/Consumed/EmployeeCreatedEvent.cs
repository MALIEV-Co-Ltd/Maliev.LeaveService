namespace Maliev.LeaveService.Domain.Events.Consumed;

public class EmployeeCreatedEvent
{
    public Guid EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public DateTimeOffset StartDate { get; set; }
    public Guid DepartmentId { get; set; }
}