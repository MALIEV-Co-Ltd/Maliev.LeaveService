using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Infrastructure.Services;
using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Leave;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service to periodically accrue leave balances for employees.
/// </summary>
public class LeaveAccrualBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LeaveAccrualBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1); // Check daily

    public LeaveAccrualBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<LeaveAccrualBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LeaveAccrualBackgroundService starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Run accrual logic on the 1st of each month
                if (DateTime.UtcNow.Day == 1)
                {
                    await ProcessAccrualsAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during leave accrual process");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("LeaveAccrualBackgroundService stopping");
    }

    private async Task ProcessAccrualsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Maliev.LeaveService.Infrastructure.Data.LeaveDbContext>();

        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;

        // Idempotency check: Check if accrual already ran for this month
        if (await context.AccrualRuns.AnyAsync(r => r.Year == currentYear && r.Month == currentMonth && r.IsSuccess, cancellationToken))
        {
            _logger.LogInformation("Leave accrual for {Month}/{Year} already processed. Skipping.", currentMonth, currentYear);
            return;
        }

        _logger.LogInformation("Starting monthly leave accrual process for {Month}/{Year}", currentMonth, currentYear);
        var startTime = DateTime.UtcNow;

        var balanceRepository = scope.ServiceProvider.GetRequiredService<ILeaveBalanceRepository>();
        var policyRepository = scope.ServiceProvider.GetRequiredService<ILeavePolicyRepository>();
        var employeeClient = scope.ServiceProvider.GetRequiredService<IEmployeeServiceClient>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var activePolicies = await policyRepository.GetAllAsync(cancellationToken);
        var activeEmployeeIds = await employeeClient.GetActiveEmployeeIdsAsync(cancellationToken);

        int processedCount = 0;
        foreach (var employeeId in activeEmployeeIds)
        {
            var balances = await balanceRepository.GetByEmployeeIdAsync(employeeId, currentYear, cancellationToken);

            foreach (var balance in balances)
            {
                bool updated = false;

                // 1. Handle Expiration (FR-019)
                if (balance.CarriedForward > 0 && balance.ExpirationDate.HasValue && balance.ExpirationDate.Value < DateTimeOffset.UtcNow)
                {
                    _logger.LogInformation("Expiring {Days} carried forward days for employee {EmployeeId}, type {LeaveType}",
                        balance.CarriedForward, employeeId, balance.LeaveType);
                    balance.CarriedForward = 0;
                    updated = true;
                }

                // 2. Handle Accrual
                var policy = activePolicies.FirstOrDefault(p => p.LeaveType == balance.LeaveType);
                if (policy != null && policy.IsActive && policy.AccrualRate > 0)
                {
                    balance.Entitled += policy.AccrualRate;
                    updated = true;
                }

                if (updated)
                {
                    await balanceRepository.UpdateAsync(balance, cancellationToken);

                    await publishEndpoint.Publish(new LeaveBalanceAdjustedEvent(
                        MessageId: Guid.NewGuid(),
                        MessageName: nameof(LeaveBalanceAdjustedEvent),
                        MessageType: MessageType.Event,
                        MessageVersion: "1.0",
                        PublishedBy: "LeaveService",
                        ConsumedBy: new List<string> { "PayrollService" },
                        CorrelationId: Guid.NewGuid(),
                        CausationId: null,
                        OccurredAtUtc: DateTimeOffset.UtcNow,
                        IsPublic: false,
                        Payload: new LeaveBalanceAdjustedEventPayload(
                            EmployeeId: employeeId,
                            LeaveType: balance.LeaveType.ToString(),
                            Year: currentYear,
                            NewEntitled: (double)balance.Entitled,
                            NewUsed: (double)balance.Used,
                            NewPending: (double)balance.Pending,
                            NewCarriedForward: (double)balance.CarriedForward)
                    ), cancellationToken);

                    processedCount++;
                }
            }
        }

        // Record successful run
        context.AccrualRuns.Add(new AccrualRun
        {
            Id = Guid.NewGuid(),
            Year = currentYear,
            Month = currentMonth,
            RunAt = DateTimeOffset.UtcNow,
            EmployeesProcessed = processedCount,
            IsSuccess = true
        });
        await context.SaveChangesAsync(cancellationToken);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation("Completed leave accrual process. Processed {Count} updates in {Duration}ms",
            processedCount, duration.TotalMilliseconds);
    }
}
