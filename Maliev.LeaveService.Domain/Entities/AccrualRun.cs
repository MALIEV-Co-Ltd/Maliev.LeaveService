namespace Maliev.LeaveService.Domain.Entities;

/// <summary>
/// Tracks the history of automated leave accrual runs.
/// </summary>
public class AccrualRun
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTimeOffset RunAt { get; set; }
    public int EmployeesProcessed { get; set; }
    public bool IsSuccess { get; set; }
}
