namespace Maliev.LeaveService.Application.Interfaces;

public interface INotificationService
{
    Task NotifyLeaveRequestSubmittedAsync(Guid requestId);
    Task NotifyLeaveRequestDecisionAsync(Guid requestId);
    Task NotifyLeaveCancellationAsync(Guid requestId);
    Task NotifyExpirationAlertAsync(Guid employeeId, int daysToExpiration);
}
