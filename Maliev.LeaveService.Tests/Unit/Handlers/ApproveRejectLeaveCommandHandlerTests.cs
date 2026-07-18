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

public class ApproveRejectLeaveCommandHandlerTests
{
    private readonly Mock<ILeaveRequestRepository> _requestRepositoryMock;
    private readonly Mock<ILeaveBalanceRepository> _balanceRepositoryMock;
    private readonly Mock<ILeaveApprovalRepository> _approvalRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<ApproveRejectLeaveCommandHandler>> _loggerMock;
    private readonly ApproveRejectLeaveCommandHandler _handler;

    public ApproveRejectLeaveCommandHandlerTests()
    {
        _requestRepositoryMock = new Mock<ILeaveRequestRepository>();
        _balanceRepositoryMock = new Mock<ILeaveBalanceRepository>();
        _approvalRepositoryMock = new Mock<ILeaveApprovalRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<ApproveRejectLeaveCommandHandler>>();

        _handler = new ApproveRejectLeaveCommandHandler(
            _requestRepositoryMock.Object,
            _balanceRepositoryMock.Object,
            _approvalRepositoryMock.Object,
            _notificationServiceMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_Approve_ShouldUpdateStatusAndBalance()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var command = new ApproveRejectLeaveCommand
        {
            RequestId = requestId,
            ApproverId = Guid.NewGuid(),
            Decision = ApprovalStatus.Approved,
            Comments = "Have a nice trip"
        };

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
            Used = 0,
            Entitled = 20
        };

        _requestRepositoryMock.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);

        _balanceRepositoryMock.Setup(r => r.GetByEmployeeAndTypeAsync(employeeId, LeaveType.Annual, leaveRequest.StartDate.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LeaveRequestStatus.Approved, leaveRequest.Status);
        Assert.Equal(0, balance.Pending);
        Assert.Equal(5, balance.Used);

        _requestRepositoryMock.Verify(r => r.UpdateAsync(leaveRequest, It.IsAny<CancellationToken>()), Times.Once);
        _balanceRepositoryMock.Verify(r => r.UpdateAsync(balance, It.IsAny<CancellationToken>()), Times.Once);
        _approvalRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LeaveApproval>(), It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<LeaveRequestApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(n => n.NotifyLeaveRequestDecisionAsync(requestId), Times.Once);
    }

    [Fact]
    public async Task Handle_Reject_ShouldReturnDaysToBalance()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var command = new ApproveRejectLeaveCommand
        {
            RequestId = requestId,
            ApproverId = Guid.NewGuid(),
            Decision = ApprovalStatus.Rejected,
            Comments = "Team coverage required"
        };

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
            Used = 0,
            Entitled = 20
        };

        _requestRepositoryMock.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);

        _balanceRepositoryMock.Setup(r => r.GetByEmployeeAndTypeAsync(employeeId, LeaveType.Annual, leaveRequest.StartDate.Year, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LeaveRequestStatus.Rejected, leaveRequest.Status);
        Assert.Equal(0, balance.Pending);
        Assert.Equal(0, balance.Used);

        _requestRepositoryMock.Verify(r => r.UpdateAsync(leaveRequest, It.IsAny<CancellationToken>()), Times.Once);
        _balanceRepositoryMock.Verify(r => r.UpdateAsync(balance, It.IsAny<CancellationToken>()), Times.Once);
        _approvalRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LeaveApproval>(), It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<LeaveRequestRejectedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
