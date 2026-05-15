using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Leave;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Application.Commands.Handlers;

public class ApproveRejectLeaveCommandHandler : IRequestHandler<ApproveRejectLeaveCommand, CommandResult>
{
    private readonly ILeaveRequestRepository _requestRepository;
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly ILeaveApprovalRepository _approvalRepository;
    private readonly INotificationService _notificationService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ApproveRejectLeaveCommandHandler> _logger;

    public ApproveRejectLeaveCommandHandler(
        ILeaveRequestRepository requestRepository,
        ILeaveBalanceRepository balanceRepository,
        ILeaveApprovalRepository approvalRepository,
        INotificationService notificationService,
        IPublishEndpoint publishEndpoint,
        ILogger<ApproveRejectLeaveCommandHandler> logger)
    {
        _requestRepository = requestRepository;
        _balanceRepository = balanceRepository;
        _approvalRepository = approvalRepository;
        _notificationService = notificationService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<CommandResult> Handle(ApproveRejectLeaveCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing decision {Decision} for request {RequestId} by approver {ApproverId}",
            request.Decision, request.RequestId, request.ApproverId);

        var leaveRequest = await _requestRepository.GetByIdAsync(request.RequestId, cancellationToken);
        if (leaveRequest == null)
        {
            _logger.LogWarning("Request {RequestId} not found for decision", request.RequestId);
            return CommandResult.Failure("Leave request not found.");
        }

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
        {
            _logger.LogWarning("Cannot process decision for request {RequestId} with status {Status}",
                request.RequestId, leaveRequest.Status);
            return CommandResult.Failure($"Cannot process a request with status {leaveRequest.Status}.");
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

        var approval = leaveRequest.Approvals.FirstOrDefault(a =>
                a.Status == ApprovalStatus.Pending && a.ApproverId == request.ApproverId)
            ?? leaveRequest.Approvals.FirstOrDefault(a => a.Status == ApprovalStatus.Pending);
        var isNewApproval = approval is null;
        approval ??= new LeaveApproval
        {
            Id = Guid.NewGuid(),
            LeaveRequestId = leaveRequest.Id
        };
        approval.ApproverId = request.ApproverId;
        approval.Status = request.Decision;
        approval.Comments = request.Comments;
        approval.DecidedAt = DateTimeOffset.UtcNow;

        if (request.Decision == ApprovalStatus.Approved)
        {
            leaveRequest.Status = LeaveRequestStatus.Approved;

            // Move Pending to Used
            balance.Pending -= leaveRequest.TotalDays;
            balance.Used += leaveRequest.TotalDays;

            await _publishEndpoint.Publish(new LeaveRequestApprovedEvent(
                MessageId: Guid.NewGuid(),
                MessageName: nameof(LeaveRequestApprovedEvent),
                MessageType: MessageType.Event,
                MessageVersion: "1.0",
                PublishedBy: "LeaveService",
                ConsumedBy: new List<string> { "NotificationService" },
                CorrelationId: Guid.NewGuid(),
                CausationId: null,
                OccurredAtUtc: DateTimeOffset.UtcNow,
                IsPublic: false,
                Payload: new LeaveRequestApprovedEventPayload(
                    RequestId: leaveRequest.Id,
                    EmployeeId: leaveRequest.EmployeeId,
                    ApproverId: request.ApproverId,
                    LeaveType: leaveRequest.LeaveType.ToString(),
                    StartDate: leaveRequest.StartDate,
                    EndDate: leaveRequest.EndDate,
                    ApprovedAt: DateTimeOffset.UtcNow)
            ), cancellationToken);
        }
        else
        {
            leaveRequest.Status = LeaveRequestStatus.Rejected;

            // Return Pending to Available
            balance.Pending -= leaveRequest.TotalDays;

            await _publishEndpoint.Publish(new LeaveRequestRejectedEvent(
                MessageId: Guid.NewGuid(),
                MessageName: nameof(LeaveRequestRejectedEvent),
                MessageType: MessageType.Event,
                MessageVersion: "1.0",
                PublishedBy: "LeaveService",
                ConsumedBy: new List<string> { "NotificationService" },
                CorrelationId: Guid.NewGuid(),
                CausationId: null,
                OccurredAtUtc: DateTimeOffset.UtcNow,
                IsPublic: false,
                Payload: new LeaveRequestRejectedEventPayload(
                    RequestId: leaveRequest.Id,
                    EmployeeId: leaveRequest.EmployeeId,
                    Reason: request.Comments ?? string.Empty,
                    RejectedAt: DateTimeOffset.UtcNow)
            ), cancellationToken);
        }

        leaveRequest.UpdatedAt = DateTimeOffset.UtcNow;

        await _requestRepository.UpdateAsync(leaveRequest, cancellationToken);
        await _balanceRepository.UpdateAsync(balance, cancellationToken);
        if (isNewApproval)
        {
            await _approvalRepository.AddAsync(approval, cancellationToken);
        }

        _logger.LogInformation("Decision {Decision} recorded for request {RequestId} (Audit: FR-027 compliant)",
            request.Decision, leaveRequest.Id);

        // Notify Employee
        await _notificationService.NotifyLeaveRequestDecisionAsync(leaveRequest.Id);

        return CommandResult.Success(leaveRequest.Id);
    }
}
