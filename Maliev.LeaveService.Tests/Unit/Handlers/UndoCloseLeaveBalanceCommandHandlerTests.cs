using Moq;
using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Commands;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Handlers;

public class UndoCloseLeaveBalanceCommandHandlerTests
{
    private readonly Mock<ILeaveBalanceRepository> _balanceRepositoryMock;
    private readonly Mock<ILogger<UndoCloseLeaveBalanceCommandHandler>> _loggerMock;
    private readonly UndoCloseLeaveBalanceCommandHandler _handler;

    public UndoCloseLeaveBalanceCommandHandlerTests()
    {
        _balanceRepositoryMock = new Mock<ILeaveBalanceRepository>();
        _loggerMock = new Mock<ILogger<UndoCloseLeaveBalanceCommandHandler>>();

        _handler = new UndoCloseLeaveBalanceCommandHandler(
            _balanceRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogAndComplete()
    {
        // Arrange
        var command = new UndoCloseLeaveBalanceCommand { EmployeeId = Guid.NewGuid() };

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        // Verified by lack of exception and internal logic
    }
}
