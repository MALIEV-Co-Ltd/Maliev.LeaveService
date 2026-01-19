using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Application.Commands.Handlers;

public class SubmitLeaveRequestCommandHandler : IRequestHandler<SubmitLeaveRequestCommand, CommandResult>
{
    private readonly ILeaveRequestRepository _requestRepository;
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly ILeavePolicyRepository _policyRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SubmitLeaveRequestCommandHandler> _logger;

    public SubmitLeaveRequestCommandHandler(
        ILeaveRequestRepository requestRepository,
        ILeaveBalanceRepository balanceRepository,
        ILeavePolicyRepository policyRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<SubmitLeaveRequestCommandHandler> logger)
    {
        _requestRepository = requestRepository;
        _balanceRepository = balanceRepository;
        _policyRepository = policyRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<CommandResult> Handle(SubmitLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing leave request submission for employee {EmployeeId}, type {LeaveType}",
            request.EmployeeId, request.LeaveType);

        // 1. Basic Date Validation
        if (request.StartDate > request.EndDate)
        {
            _logger.LogWarning("Invalid leave dates: Start {StartDate} > End {EndDate}", request.StartDate, request.EndDate);
            return CommandResult.Failure("Start date cannot be after end date.");
        }

        var duration = (request.EndDate - request.StartDate).TotalDays + 1;
        if (duration > 30)
        {
            _logger.LogWarning("Leave duration {Duration} exceeds maximum of 30 days", duration);
            return CommandResult.Failure("Leave duration cannot exceed 30 consecutive days.");
        }

        // 2. Advance Notice Validation (FR-003, FR-022)
        if (request.LeaveType != LeaveType.Sick)
        {
            if (request.StartDate < DateTimeOffset.UtcNow.AddHours(24))
            {
                _logger.LogWarning("Insufficient notice for {LeaveType}: {StartDate}", request.LeaveType, request.StartDate);
                return CommandResult.Failure("Non-sick leave requires at least 24 hours advance notice.");
            }
        }

        // 3. Overlap Detection (FR-003)
        var hasOverlap = await _requestRepository.HasOverlapAsync(request.EmployeeId, request.StartDate, request.EndDate, cancellationToken);
        if (hasOverlap)
        {
            _logger.LogWarning("Overlapping leave request detected for employee {EmployeeId} between {StartDate} and {EndDate}",
                request.EmployeeId, request.StartDate, request.EndDate);
            return CommandResult.Failure("Selected dates overlap with an existing leave request.");
        }

        // 4. Balance Validation (FR-004)
        var year = request.StartDate.Year;
        var balance = await _balanceRepository.GetByEmployeeAndTypeAsync(request.EmployeeId, request.LeaveType, year, cancellationToken);

        if (balance == null)
        {
            _logger.LogWarning("No leave balance found for employee {EmployeeId}, type {LeaveType}, year {Year}",
                request.EmployeeId, request.LeaveType, year);
            return CommandResult.Failure($"No leave balance found for {request.LeaveType} in {year}.");
        }

        decimal requestedDays = (request.HalfDayPeriod != HalfDayPeriod.FullDay) ? 0.5m : (decimal)duration;

        if (balance.Available < requestedDays)
        {
            _logger.LogWarning("Insufficient balance for employee {EmployeeId}: Required {Requested}, Available {Available}",
                request.EmployeeId, requestedDays, balance.Available);
            return CommandResult.Failure("Insufficient balance for this leave request.");
        }

        // 5. Policy Check
        var policy = await _policyRepository.GetByTypeAsync(request.LeaveType, cancellationToken);
        if (policy != null && !policy.IsActive)
        {
            _logger.LogWarning("Attempted to request inactive leave type {LeaveType}", request.LeaveType);
            return CommandResult.Failure($"Leave type {request.LeaveType} is currently inactive.");
        }

        // 6. Create Leave Request
        var leaveRequest = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = requestedDays,
            HalfDayPeriod = request.HalfDayPeriod,
            Reason = request.Reason,
            Status = LeaveRequestStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // 7. Update Balance (Move to Pending)
        balance.Pending += requestedDays;

        // 8. Persist Changes
        await _requestRepository.AddAsync(leaveRequest, cancellationToken);
        await _balanceRepository.UpdateAsync(balance, cancellationToken);

        _logger.LogInformation("Leave request {RequestId} created for employee {EmployeeId} (Audit: FR-027 compliant)",
            leaveRequest.Id, leaveRequest.EmployeeId);

        // 9. Publish Event
        await _publishEndpoint.Publish(new Maliev.MessagingContracts.Generated.LeaveRequestSubmittedEvent(
            Guid.NewGuid(),
            nameof(Maliev.MessagingContracts.Generated.LeaveRequestSubmittedEvent),
            Maliev.MessagingContracts.Generated.MessageType.Event,
            "1.0",
            "LeaveService",
            new[] { "NotificationService" },
            Guid.NewGuid(),
            null,
            DateTimeOffset.UtcNow,
            false,
            new Maliev.MessagingContracts.Generated.LeaveRequestSubmittedEventPayload(
                leaveRequest.Id,
                leaveRequest.EmployeeId,
                leaveRequest.LeaveType.ToString(),
                leaveRequest.StartDate,
                leaveRequest.EndDate,
                (double)leaveRequest.TotalDays,
                leaveRequest.CreatedAt)
        ), cancellationToken);

        return CommandResult.Success(leaveRequest.Id);
    }
}