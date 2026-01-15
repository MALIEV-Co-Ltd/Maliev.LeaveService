using Maliev.LeaveService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service to send alerts for expiring leave balances.
/// </summary>
public class LeaveExpirationAlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LeaveExpirationAlertBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);

    public LeaveExpirationAlertBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<LeaveExpirationAlertBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LeaveExpirationAlertBackgroundService starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendExpirationAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending leave expiration alerts");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("LeaveExpirationAlertBackgroundService stopping");
    }

    private async Task SendExpirationAlertsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var balanceRepository = scope.ServiceProvider.GetRequiredService<ILeaveBalanceRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        // Find balances expiring exactly 30 days from now
        var targetDate = DateTimeOffset.UtcNow.AddDays(30);
        var expiringBalances = await balanceRepository.GetExpiringBalancesAsync(targetDate, cancellationToken);

        foreach (var balance in expiringBalances)
        {
            _logger.LogInformation("Sending expiration alert for employee {EmployeeId}, leave type {LeaveType}",
                balance.EmployeeId, balance.LeaveType);

            await notificationService.NotifyExpirationAlertAsync(balance.EmployeeId, 30);
        }
    }
}