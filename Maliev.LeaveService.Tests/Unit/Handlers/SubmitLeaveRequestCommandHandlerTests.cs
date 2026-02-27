using Moq;
using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Leave;
using MassTransit;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Handlers;

public class SubmitLeaveRequestCommandHandlerTests
{
    private readonly Mock<ILeaveRequestRepository> _requestRepositoryMock;
    private readonly Mock<ILeaveBalanceRepository> _balanceRepositoryMock;
    private readonly Mock<ILeavePolicyRepository> _policyRepositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<SubmitLeaveRequestCommandHandler>> _loggerMock;
    private readonly SubmitLeaveRequestCommandHandler _handler;

    public SubmitLeaveRequestCommandHandlerTests()
    {
        _requestRepositoryMock = new Mock<ILeaveRequestRepository>();
        _balanceRepositoryMock = new Mock<ILeaveBalanceRepository>();
        _policyRepositoryMock = new Mock<ILeavePolicyRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<SubmitLeaveRequestCommandHandler>>();

        _handler = new SubmitLeaveRequestCommandHandler(
            _requestRepositoryMock.Object,
            _balanceRepositoryMock.Object,
            _policyRepositoryMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldSaveAndReturnSuccess()
    {
        // Arrange
        var startDate = DateTimeOffset.UtcNow.AddDays(7).Date;
        var endDate = startDate.AddDays(3); // 4 days

        var command = new SubmitLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual,
            StartDate = startDate,
            EndDate = endDate,
            Reason = "Vacation"
        };

        var balance = new LeaveBalance { Entitled = 20, Used = 0, Pending = 0 };
        _balanceRepositoryMock.Setup(r => r.GetByEmployeeAndTypeAsync(command.EmployeeId, command.LeaveType, It.IsAny<int>()))
            .ReturnsAsync(balance);

        _requestRepositoryMock.Setup(r => r.HasOverlapAsync(command.EmployeeId, command.StartDate, command.EndDate))
            .ReturnsAsync(false);

        _policyRepositoryMock.Setup(r => r.GetByTypeAsync(command.LeaveType))
            .ReturnsAsync(new LeavePolicy { LeaveType = command.LeaveType, IsActive = true });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _requestRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LeaveRequest>()), Times.Once);
        _balanceRepositoryMock.Verify(r => r.UpdateAsync(It.Is<LeaveBalance>(b => b.Pending == 4)), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<LeaveRequestSubmittedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InsufficientBalance_ShouldReturnFailure()
    {
        // Arrange
        var command = new SubmitLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual,
            StartDate = DateTimeOffset.UtcNow.AddDays(7),
            EndDate = DateTimeOffset.UtcNow.AddDays(30), // 24 days
            Reason = "Too long"
        };

        var balance = new LeaveBalance { Entitled = 10, Used = 0, Pending = 0 };
        _balanceRepositoryMock.Setup(r => r.GetByEmployeeAndTypeAsync(command.EmployeeId, command.LeaveType, It.IsAny<int>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Insufficient balance", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_OverlappingRequest_ShouldReturnFailure()
    {
        // Arrange
        var command = new SubmitLeaveRequestCommand
        {
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual,
            StartDate = DateTimeOffset.UtcNow.AddDays(7),
            EndDate = DateTimeOffset.UtcNow.AddDays(10)
        };

        _requestRepositoryMock.Setup(r => r.HasOverlapAsync(command.EmployeeId, command.StartDate, command.EndDate))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("overlap", result.ErrorMessage);
    }
}
