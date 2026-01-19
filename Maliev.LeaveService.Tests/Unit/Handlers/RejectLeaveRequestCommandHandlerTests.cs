using Moq;
using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Handlers;

public class RejectLeaveRequestCommandHandlerTests
{
    private readonly Mock<ILeaveRequestRepository> _requestRepositoryMock;
    private readonly Mock<ILeaveBalanceRepository> _balanceRepositoryMock;
    private readonly Mock<ILeaveApprovalRepository> _approvalRepositoryMock;
    private readonly RejectLeaveRequestCommandHandler _handler;

    public RejectLeaveRequestCommandHandlerTests()
    {
        _requestRepositoryMock = new Mock<ILeaveRequestRepository>();
        _balanceRepositoryMock = new Mock<ILeaveBalanceRepository>();
        _approvalRepositoryMock = new Mock<ILeaveApprovalRepository>();

        _handler = new RejectLeaveRequestCommandHandler(
            _requestRepositoryMock.Object,
            _balanceRepositoryMock.Object,
            _approvalRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReject()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var command = new RejectLeaveRequestCommand { RequestId = requestId, ApproverId = approverId };

        var leaveRequest = new LeaveRequest
        {
            Id = requestId,
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual,
            StartDate = DateTimeOffset.UtcNow,
            TotalDays = 2,
            Status = LeaveRequestStatus.Pending
        };

        var balance = new LeaveBalance { Pending = 2, Used = 0 };

        _requestRepositoryMock.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);
        _balanceRepositoryMock.Setup(r => r.GetByEmployeeAndTypeAsync(leaveRequest.EmployeeId, leaveRequest.LeaveType, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LeaveRequestStatus.Rejected, leaveRequest.Status);
        Assert.Equal(0, balance.Pending);
        Assert.Equal(0, balance.Used);
        _approvalRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LeaveApproval>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
