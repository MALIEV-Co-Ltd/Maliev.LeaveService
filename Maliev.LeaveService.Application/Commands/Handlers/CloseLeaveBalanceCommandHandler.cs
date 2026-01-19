using Maliev.LeaveService.Application.Interfaces;
using Maliev.MessagingContracts.Generated;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Application.Commands.Handlers;

/// <summary>
/// Handler for CloseLeaveBalanceCommand (Saga step).
/// </summary>
public class CloseLeaveBalanceCommandHandler : IRequestHandler<CloseLeaveBalanceCommand, bool>
{
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CloseLeaveBalanceCommandHandler> _logger;

    public CloseLeaveBalanceCommandHandler(
        ILeaveBalanceRepository balanceRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<CloseLeaveBalanceCommandHandler> logger)
    {
        _balanceRepository = balanceRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<bool> Handle(CloseLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Closing leave balances for employee {EmployeeId} (Correlation: {CorrelationId})",
            request.EmployeeId, request.CorrelationId);

        var balances = await _balanceRepository.GetByEmployeeIdAsync(request.EmployeeId, DateTime.UtcNow.Year, cancellationToken);

        foreach (var balance in balances)
        {
            // Logic to mark balance as closed/inactive if applicable
            // For now, we'll just log and proceed
        }

        var leaveBalanceClosedEvent = new LeaveBalanceClosedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(LeaveBalanceClosedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "LeaveService",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: request.CorrelationId,
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new LeaveBalanceClosedEventPayload(
                EmployeeId: request.EmployeeId,
                ClosedAt: DateTimeOffset.UtcNow
            )
        );

        await _publishEndpoint.Publish(leaveBalanceClosedEvent, cancellationToken);

        return true;
    }
}
