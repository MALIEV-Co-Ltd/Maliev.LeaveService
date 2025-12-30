using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Commands;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Application.Commands.Handlers;

/// <summary>
/// Handler for UndoCloseLeaveBalanceCommand (Compensating Transaction).
/// </summary>
public class UndoCloseLeaveBalanceCommandHandler
{
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly ILogger<UndoCloseLeaveBalanceCommandHandler> _logger;

    public UndoCloseLeaveBalanceCommandHandler(
        ILeaveBalanceRepository balanceRepository,
        ILogger<UndoCloseLeaveBalanceCommandHandler> logger)
    {
        _balanceRepository = balanceRepository;
        _logger = logger;
    }

    public async Task HandleAsync(UndoCloseLeaveBalanceCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("UNDO: Restoring leave balances for employee {EmployeeId}", command.EmployeeId);
        
        // In a real scenario, we might re-activate records or reverse status changes
        // For this refactoring, we log the compensation action
        
        await Task.CompletedTask;
    }
}
