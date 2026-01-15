using Maliev.MessagingContracts.Generated;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
// using Maliev.LeaveService.Domain.Events.Consumed; // Removed
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Infrastructure.Consumers;

/// <summary>
/// Consumer for EmployeeCreatedEvent.
/// Initializes leave balances for new employees based on active leave policies.
/// </summary>
public class EmployeeCreatedEventConsumer : IConsumer<EmployeeCreatedEvent>
{
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly ILeavePolicyRepository _policyRepository;
    private readonly ILogger<EmployeeCreatedEventConsumer> _logger;

    public EmployeeCreatedEventConsumer(
        ILeaveBalanceRepository balanceRepository,
        ILeavePolicyRepository policyRepository,
        ILogger<EmployeeCreatedEventConsumer> logger)
    {
        _balanceRepository = balanceRepository;
        _policyRepository = policyRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        var @event = context.Message;
        var payload = @event.Payload; // Access payload

        _logger.LogInformation("Processing EmployeeCreatedEvent for Employee: {EmployeeId}", payload.EmployeeId);

        var currentYear = DateTime.UtcNow.Year;
        var activePolicies = await _policyRepository.GetAllAsync();

        foreach (var policy in activePolicies.Where(p => p.IsActive))
        {
            var existingBalance = await _balanceRepository.GetByEmployeeAndTypeAsync(
                payload.EmployeeId,
                policy.LeaveType,
                currentYear);

            if (existingBalance == null)
            {
                var newBalance = new LeaveBalance
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = payload.EmployeeId,
                    LeaveType = policy.LeaveType,
                    Year = currentYear,
                    Entitled = policy.DefaultEntitlement,
                    Used = 0,
                    Pending = 0,
                    CarriedForward = 0
                };

                await _balanceRepository.AddAsync(newBalance);
                _logger.LogInformation("Initialized {LeaveType} balance for employee {EmployeeId}",
                    policy.LeaveType, payload.EmployeeId);
            }
        }
    }
}
