using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Enums;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Application.Commands.Handlers;

public class CancelLeaveRequestCommandHandler : IRequestHandler<CancelLeaveRequestCommand, CommandResult>
{
    private readonly ILeaveRequestRepository _requestRepository;
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly INotificationService _notificationService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CancelLeaveRequestCommandHandler> _logger;

    public CancelLeaveRequestCommandHandler(
        ILeaveRequestRepository requestRepository,
        ILeaveBalanceRepository balanceRepository,
        INotificationService notificationService,
        IPublishEndpoint publishEndpoint,
        ILogger<CancelLeaveRequestCommandHandler> logger)
    {
        _requestRepository = requestRepository;
        _balanceRepository = balanceRepository;
        _notificationService = notificationService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<CommandResult> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing cancellation for request {RequestId} by user {RequestedBy}",
            request.RequestId, request.RequestedBy);

        var leaveRequest = await _requestRepository.GetByIdAsync(request.RequestId, cancellationToken);
        if (leaveRequest == null)
        {
            _logger.LogWarning("Request {RequestId} not found for cancellation", request.RequestId);
            return CommandResult.Failure("Leave request not found.");
        }

        if (leaveRequest.Status == LeaveRequestStatus.Cancelled || leaveRequest.Status == LeaveRequestStatus.Rejected)
        {
            _logger.LogWarning("Cannot cancel request {RequestId} with status {Status}",
                request.RequestId, leaveRequest.Status);
            return CommandResult.Failure($"Cannot cancel a request with status {leaveRequest.Status}.");
        }

        var balance = await _balanceRepository.GetByEmployeeAndTypeAsync(
            leaveRequest.EmployeeId,
            leaveRequest.LeaveType,
            leaveRequest.StartDate.Year,
            cancellationToken);

        if (balance == null)
        {
            _logger.LogWarning("Leave balance not found for employee {EmployeeId}, year {Year}",
                leaveRequest.EmployeeId, leaveRequest.StartDate.Year);
            return CommandResult.Failure("Leave balance not found.");
        }

        var oldStatus = leaveRequest.Status;

        // Update Status
        leaveRequest.Status = LeaveRequestStatus.Cancelled;
        leaveRequest.UpdatedAt = DateTimeOffset.UtcNow;

        // Revert Balance
        if (oldStatus == LeaveRequestStatus.Pending)
        {
            balance.Pending -= leaveRequest.TotalDays;
        }
        else if (oldStatus == LeaveRequestStatus.Approved)
        {
            balance.Used -= leaveRequest.TotalDays;
        }

        await _requestRepository.UpdateAsync(leaveRequest, cancellationToken);
        await _balanceRepository.UpdateAsync(balance, cancellationToken);

        _logger.LogInformation("Request {RequestId} cancelled (Audit: FR-027 compliant)", leaveRequest.Id);

        await _publishEndpoint.Publish(new Maliev.MessagingContracts.Generated.LeaveRequestCancelledEvent(
            Guid.NewGuid(),
            nameof(Maliev.MessagingContracts.Generated.LeaveRequestCancelledEvent),
            Maliev.MessagingContracts.Generated.MessageType.Event,
            "1.0",
            "LeaveService",
            new[] { "NotificationService" },
            Guid.NewGuid(),
            null,
            DateTimeOffset.UtcNow,
            false,
            new Maliev.MessagingContracts.Generated.LeaveRequestCancelledEventPayload(
                leaveRequest.Id,
                leaveRequest.EmployeeId,
                request.Comments ?? string.Empty,
                DateTimeOffset.UtcNow)
        ), cancellationToken);

        if (oldStatus == LeaveRequestStatus.Approved)
        {
            await _notificationService.NotifyLeaveCancellationAsync(leaveRequest.Id);
        }

        return CommandResult.Success(leaveRequest.Id);
    }
}