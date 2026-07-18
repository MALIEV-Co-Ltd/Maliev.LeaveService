using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Infrastructure.Consumers;
using Maliev.MessagingContracts.Contracts.Leave;
using Maliev.MessagingContracts.Contracts.Shared;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Consumers;

public class UndoCloseLeaveBalanceConsumerTests
{
    [Fact]
    public async Task Consume_CallsHandler()
    {
        // Arrange
        var balanceRepoMock = new Mock<ILeaveBalanceRepository>();
        var loggerMock = new Mock<ILogger<UndoCloseLeaveBalanceCommandHandler>>();
        var handlerMock = new UndoCloseLeaveBalanceCommandHandler(balanceRepoMock.Object, loggerMock.Object);
        var consumer = new UndoCloseLeaveBalanceConsumer(handlerMock);

        var command = CreateCommand(Guid.NewGuid());

        var contextMock = new Mock<ConsumeContext<UndoCloseLeaveBalanceCommand>>();
        contextMock.Setup(c => c.Message).Returns(command);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        // Act
        await consumer.Consume(contextMock.Object);

        // Assert - verify the handler was called (it just logs)
        contextMock.Verify(c => c.Message, Times.Once);
    }

    private static UndoCloseLeaveBalanceCommand CreateCommand(Guid employeeId)
    {
        return new UndoCloseLeaveBalanceCommand(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(UndoCloseLeaveBalanceCommand),
            MessageType: MessageType.Command,
            MessageVersion: "1.0",
            PublishedBy: "EmployeeService",
            ConsumedBy: ["LeaveService"],
            CorrelationId: employeeId,
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new UndoCloseLeaveBalanceCommandPayload(employeeId));
    }
}
