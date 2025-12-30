namespace Maliev.LeaveService.Domain.Commands;

/// <summary>
/// Command to restore leave balances if a termination saga fails (Compensating Transaction)
/// </summary>
public record UndoCloseLeaveBalanceCommand
{
    public Guid EmployeeId { get; init; }
}
