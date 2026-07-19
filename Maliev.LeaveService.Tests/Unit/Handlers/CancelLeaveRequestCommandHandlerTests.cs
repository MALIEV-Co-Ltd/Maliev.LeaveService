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

public class CancelLeaveRequestCommandHandlerTests
{
    private readonly Mock<ILeaveRequestRepository> _requestRepositoryMock;
    private readonly Mock<ILeaveBalanceRepository> _balanceRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<CancelLeaveRequestCommandHandler>> _loggerMock;
    private readonly CancelLeaveRequestCommandHandler _handler;

    public CancelLeaveRequestCommandHandlerTests()
    {
        _requestRepositoryMock = new Mock<ILeaveRequestRepository>();
        _balanceRepositoryMock = new Mock<ILeaveBalanceRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<CancelLeaveRequestCommandHandler>>();

        _handler = new CancelLeaveRequestCommandHandler(
            _requestRepositoryMock.Object,
            _balanceRepositoryMock.Object,
            _notificationServiceMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_PendingRequest_ShouldUpdateStatusAndReturnPendingDays()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var command = new CancelLeaveRequestCommand { RequestId = requestId, RequestedBy = employeeId };

        var leaveRequest = new LeaveRequest
        {
            Id = requestId,
            EmployeeId = employeeId,
            LeaveType = LeaveType.Annual,
            TotalDays = 5,
            Status = LeaveRequestStatus.Pending,
            StartDate = DateTimeOffset.UtcNow.AddDays(7)
        };

        var balance = new LeaveBalance
        {
            EmployeeId = employeeId,
            LeaveType = LeaveType.Annual,
            Pending = 5,
            Used = 0
        };

        _requestRepositoryMock.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);

        _balanceRepositoryMock.Setup(r => r.GetByEmployeeAndTypeAsync(employeeId, LeaveType.Annual, leaveRequest.StartDate.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LeaveRequestStatus.Cancelled, leaveRequest.Status);
        Assert.Equal(0, balance.Pending);

        _requestRepositoryMock.Verify(r => r.UpdateAsync(leaveRequest, It.IsAny<CancellationToken>()), Times.Once);
        _balanceRepositoryMock.Verify(r => r.UpdateAsync(balance, It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<LeaveRequestCancelledEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ApprovedRequest_ShouldUpdateStatusAndReturnUsedDays()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var command = new CancelLeaveRequestCommand { RequestId = requestId, RequestedBy = employeeId };

        var leaveRequest = new LeaveRequest
        {
            Id = requestId,
            EmployeeId = employeeId,
            LeaveType = LeaveType.Annual,
            TotalDays = 5,
            Status = LeaveRequestStatus.Approved,
            StartDate = DateTimeOffset.UtcNow.AddDays(7)
        };

        var balance = new LeaveBalance
        {
            EmployeeId = employeeId,
            LeaveType = LeaveType.Annual,
            Pending = 0,
            Used = 5
        };

        _requestRepositoryMock.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);

        _balanceRepositoryMock.Setup(r => r.GetByEmployeeAndTypeAsync(employeeId, LeaveType.Annual, leaveRequest.StartDate.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LeaveRequestStatus.Cancelled, leaveRequest.Status);
        Assert.Equal(0, balance.Used);

        _requestRepositoryMock.Verify(r => r.UpdateAsync(leaveRequest, It.IsAny<CancellationToken>()), Times.Once);
        _balanceRepositoryMock.Verify(r => r.UpdateAsync(balance, It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(n => n.NotifyLeaveCancellationAsync(requestId), Times.Once);
    }
}
