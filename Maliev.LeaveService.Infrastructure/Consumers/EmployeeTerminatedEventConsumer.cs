using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Domain.Events.Consumed;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Infrastructure.Consumers;

/// <summary>
/// Consumer for EmployeeTerminatedEvent.
/// Cancels all pending leave requests for terminated employees.
/// </summary>
public class EmployeeTerminatedEventConsumer : IConsumer<EmployeeTerminatedEvent>
{
    private readonly ILeaveRequestRepository _requestRepository;
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly ILogger<EmployeeTerminatedEventConsumer> _logger;

    public EmployeeTerminatedEventConsumer(
        ILeaveRequestRepository requestRepository,
        ILeaveBalanceRepository balanceRepository,
        ILogger<EmployeeTerminatedEventConsumer> logger)
    {
        _requestRepository = requestRepository;
        _balanceRepository = balanceRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmployeeTerminatedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("Processing EmployeeTerminatedEvent for Employee: {EmployeeId}", @event.EmployeeId);

        var pendingRequests = await _requestRepository.GetByEmployeeIdAsync(@event.EmployeeId);
        var activePending = pendingRequests.Where(r => r.Status == LeaveRequestStatus.Pending);

        foreach (var request in activePending)
        {
            request.Status = LeaveRequestStatus.Cancelled;
            request.UpdatedAt = DateTimeOffset.UtcNow;

            var balance = await _balanceRepository.GetByEmployeeAndTypeAsync(
                request.EmployeeId, 
                request.LeaveType, 
                request.StartDate.Year);

            if (balance != null)
            {
                balance.Pending -= request.TotalDays;
                await _balanceRepository.UpdateAsync(balance);
            }

            await _requestRepository.UpdateAsync(request);
            _logger.LogInformation("Cancelled pending leave request {RequestId} due to employee termination", request.Id);
        }
    }
}
