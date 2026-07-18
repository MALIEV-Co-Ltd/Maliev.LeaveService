using Moq;
using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Leave;
using MassTransit;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Handlers;

public class CloseLeaveBalanceCommandHandlerTests
{
    private readonly Mock<ILeaveBalanceRepository> _balanceRepositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<CloseLeaveBalanceCommandHandler>> _loggerMock;
    private readonly CloseLeaveBalanceCommandHandler _handler;

    public CloseLeaveBalanceCommandHandlerTests()
    {
        _balanceRepositoryMock = new Mock<ILeaveBalanceRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<CloseLeaveBalanceCommandHandler>>();

        _handler = new CloseLeaveBalanceCommandHandler(
            _balanceRepositoryMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldPublishEventAndReturnTrue()
    {
        // Arrange
        var command = new CloseLeaveBalanceCommand(Guid.NewGuid(), Guid.NewGuid());
        _balanceRepositoryMock.Setup(r => r.GetByEmployeeIdAsync(command.EmployeeId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LeaveBalance>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<LeaveBalanceClosedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
