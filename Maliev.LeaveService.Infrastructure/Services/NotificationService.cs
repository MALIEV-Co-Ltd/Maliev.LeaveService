using Maliev.LeaveService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyLeaveRequestSubmittedAsync(Guid requestId)
    {
        _logger.LogInformation("Notifying submission for request {RequestId}", requestId);
        // await _httpClient.PostAsJsonAsync("api/notifications/leave-submitted", new { requestId });
        await Task.CompletedTask;
    }

    public async Task NotifyLeaveRequestDecisionAsync(Guid requestId)
    {
        _logger.LogInformation("Notifying decision for request {RequestId}", requestId);
        // await _httpClient.PostAsJsonAsync("api/notifications/leave-decision", new { requestId });
        await Task.CompletedTask;
    }

    public async Task NotifyLeaveCancellationAsync(Guid requestId)
    {
        _logger.LogInformation("Notifying cancellation for request {RequestId}", requestId);
        // await _httpClient.PostAsJsonAsync("api/notifications/leave-cancelled", new { requestId });
        await Task.CompletedTask;
    }

    public async Task NotifyExpirationAlertAsync(Guid employeeId, int daysToExpiration)
    {
        _logger.LogInformation("Notifying expiration alert for employee {EmployeeId}", employeeId);
        // await _httpClient.PostAsJsonAsync("api/notifications/leave-expiration", new { employeeId, daysToExpiration });
        await Task.CompletedTask;
    }
}