using MediatR;

namespace Maliev.LeaveService.Application.Commands;

/// <summary>
/// Saga command to close all active leave balances for an employee.
/// </summary>
public record CloseLeaveBalanceCommand(Guid EmployeeId, Guid CorrelationId) : IRequest<bool>;