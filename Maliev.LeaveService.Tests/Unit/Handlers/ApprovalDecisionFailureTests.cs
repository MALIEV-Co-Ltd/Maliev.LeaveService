using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Maliev.MessagingContracts.Contracts.Leave;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Maliev.LeaveService.Tests.Unit.Handlers;

public sealed class ApprovalDecisionFailureTests
{
    private readonly Mock<ILeaveRequestRepository> _requests = new();
    private readonly Mock<ILeaveBalanceRepository> _balances = new();
    private readonly Mock<ILeaveApprovalRepository> _approvals = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly Mock<IPublishEndpoint> _events = new();

    private ApproveRejectLeaveCommandHandler CreateHandler() => new(
        _requests.Object, _balances.Object, _approvals.Object, _notifications.Object,
        _events.Object, NullLogger<ApproveRejectLeaveCommandHandler>.Instance);

    [Fact]
    public async Task MissingRequestDoesNotChangeBalancesOrPublishDecision()
    {
        var command = new ApproveRejectLeaveCommand { RequestId = Guid.NewGuid(), Decision = ApprovalStatus.Approved };

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Leave request not found.", result.ErrorMessage);
        _balances.VerifyNoOtherCalls();
        _approvals.VerifyNoOtherCalls();
        _events.VerifyNoOtherCalls();
        _notifications.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AlreadyDecidedRequestDoesNotChangeBalancesOrPublishAgain()
    {
        var request = new LeaveRequest { Id = Guid.NewGuid(), Status = LeaveRequestStatus.Approved };
        _requests.Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var result = await CreateHandler().Handle(new ApproveRejectLeaveCommand { RequestId = request.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal($"Cannot process a request with status {LeaveRequestStatus.Approved}.", result.ErrorMessage);
        _balances.VerifyNoOtherCalls();
        _approvals.VerifyNoOtherCalls();
        _events.VerifyNoOtherCalls();
        _notifications.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingBalanceLeavesPendingRequestUnchanged()
    {
        var request = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            Status = LeaveRequestStatus.Pending,
            LeaveType = LeaveType.Annual,
            StartDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
        _requests.Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);

        var result = await CreateHandler().Handle(new ApproveRejectLeaveCommand { RequestId = request.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Leave balance not found.", result.ErrorMessage);
        Assert.Equal(LeaveRequestStatus.Pending, request.Status);
        _requests.Verify(x => x.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _approvals.VerifyNoOtherCalls();
        _events.VerifyNoOtherCalls();
        _notifications.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExistingPendingApprovalIsUpdatedWithoutCreatingDuplicate()
    {
        var approver = Guid.NewGuid();
        var existing = new LeaveApproval { Id = Guid.NewGuid(), ApproverId = approver, Status = ApprovalStatus.Pending };
        var request = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            Status = LeaveRequestStatus.Pending,
            LeaveType = LeaveType.Annual,
            StartDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            TotalDays = 2,
            Approvals = new List<LeaveApproval> { existing }
        };
        var balance = new LeaveBalance { Pending = 2, Used = 1 };
        _requests.Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _balances.Setup(x => x.GetByEmployeeAndTypeAsync(request.EmployeeId, request.LeaveType, 2026, It.IsAny<CancellationToken>())).ReturnsAsync(balance);

        var result = await CreateHandler().Handle(new ApproveRejectLeaveCommand
        { RequestId = request.Id, ApproverId = approver, Decision = ApprovalStatus.Approved, Comments = "Approved" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalStatus.Approved, existing.Status);
        Assert.Equal("Approved", existing.Comments);
        Assert.Equal(0, balance.Pending);
        Assert.Equal(3, balance.Used);
        _approvals.Verify(x => x.AddAsync(It.IsAny<LeaveApproval>(), It.IsAny<CancellationToken>()), Times.Never);
        _events.Verify(x => x.Publish(It.IsAny<LeaveRequestApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
