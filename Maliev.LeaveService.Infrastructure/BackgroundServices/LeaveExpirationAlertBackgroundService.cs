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
        _logger.LogInformation("LeaveExpirationAlertBackgroundService starting - waiting 15 seconds for infrastructure");

        // Initial delay to allow infrastructure (database, services) to warm up
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        // First execution with retry logic
        var maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(5);

        for (int attempt = 1; attempt <= maxRetries && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                _logger.LogInformation("Initial expiration alert check attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                await SendExpirationAlertsAsync(stoppingToken);
                _logger.LogInformation("Initial expiration alert check completed successfully");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Initial expiration alert check attempt {Attempt}/{MaxRetries} failed", attempt, maxRetries);

                if (attempt < maxRetries)
                {
                    _logger.LogInformation("Retrying in {RetryDelay} seconds", retryDelay.TotalSeconds);
                    await Task.Delay(retryDelay, stoppingToken);
                    retryDelay = TimeSpan.FromSeconds(retryDelay.TotalSeconds * 2); // Exponential backoff
                }
                else
                {
                    _logger.LogError(ex, "Initial expiration alert check failed after {MaxRetries} attempts", maxRetries);
                }
            }
        }

        // Continue with regular scheduled checks
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_checkInterval, stoppingToken);

            try
            {
                await SendExpirationAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending leave expiration alerts");
            }
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
