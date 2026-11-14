using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Maliev.LeaveService.Tests.Unit.Handlers;

public sealed class LeaveCancellationFailureTests
{
    [Theory]
    [InlineData(LeaveRequestStatus.Cancelled)]
    [InlineData(LeaveRequestStatus.Rejected)]
    public async Task AlreadyClosedRequestCannotRestoreBalanceOrPublishAgain(LeaveRequestStatus status)
    {
        var request = new LeaveRequest { Id = Guid.NewGuid(), Status = status };
        var requests = new Mock<ILeaveRequestRepository>();
        requests.Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        var balances = new Mock<ILeaveBalanceRepository>();
        var notifications = new Mock<INotificationService>();
        var events = new Mock<IPublishEndpoint>();
        var handler = new CancelLeaveRequestCommandHandler(requests.Object, balances.Object, notifications.Object,
            events.Object, NullLogger<CancelLeaveRequestCommandHandler>.Instance);

        var result = await handler.Handle(new CancelLeaveRequestCommand { RequestId = request.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal($"Cannot cancel a request with status {status}.", result.ErrorMessage);
        Assert.Equal(status, request.Status);
        balances.VerifyNoOtherCalls();
        events.VerifyNoOtherCalls();
        notifications.VerifyNoOtherCalls();
        requests.Verify(x => x.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
