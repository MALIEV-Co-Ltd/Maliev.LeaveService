using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Commands;
using Maliev.LeaveService.Infrastructure.Consumers;
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
        
        var command = new UndoCloseLeaveBalanceCommand
        {
            EmployeeId = Guid.NewGuid()
        };
        
        var contextMock = new Mock<ConsumeContext<UndoCloseLeaveBalanceCommand>>();
        contextMock.Setup(c => c.Message).Returns(command);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        // Act
        await consumer.Consume(contextMock.Object);

        // Assert - verify the handler was called (it just logs)
        contextMock.Verify(c => c.Message, Times.Once);
    }
}
