using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Infrastructure.Services;
using MassTransit;
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
        _logger.LogInformation("Starting monthly leave accrual process");
        var startTime = DateTime.UtcNow;

        using var scope = _serviceProvider.CreateScope();
        var balanceRepository = scope.ServiceProvider.GetRequiredService<ILeaveBalanceRepository>();
        var policyRepository = scope.ServiceProvider.GetRequiredService<ILeavePolicyRepository>();
        var employeeClient = scope.ServiceProvider.GetRequiredService<EmployeeServiceClient>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var activePolicies = await policyRepository.GetAllAsync(cancellationToken);
        var activeEmployeeIds = await employeeClient.GetActiveEmployeeIdsAsync(cancellationToken);
        var currentYear = DateTime.UtcNow.Year;

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
                    
                    await publishEndpoint.Publish(new Domain.Events.Published.LeaveBalanceAdjustedEvent
                    {
                        EmployeeId = employeeId,
                        LeaveType = balance.LeaveType,
                        Year = currentYear,
                        NewEntitled = balance.Entitled,
                        NewUsed = balance.Used,
                        NewPending = balance.Pending,
                        NewCarriedForward = balance.CarriedForward
                    }, cancellationToken);
                    
                    processedCount++;
                }
            }
        }

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation("Completed leave accrual process. Processed {Count} updates in {Duration}ms", 
            processedCount, duration.TotalMilliseconds);
    }
}