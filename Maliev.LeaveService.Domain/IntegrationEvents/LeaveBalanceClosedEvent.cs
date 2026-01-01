namespace Maliev.LeaveService.Domain.IntegrationEvents;

/// <summary>
/// Event published when an employee's leave balances have been successfully closed.
/// </summary>
public record LeaveBalanceClosedEvent(Guid EmployeeId, Guid CorrelationId);
