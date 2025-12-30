using Moq;
using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Application.Queries.Handlers;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Handlers;

public class GetLeaveRequestsQueryHandlerTests
{
    private readonly Mock<ILeaveRequestRepository> _requestRepositoryMock;
    private readonly Mock<ILogger<GetLeaveRequestsQueryHandler>> _loggerMock;
    private readonly GetLeaveRequestsQueryHandler _handler;

    public GetLeaveRequestsQueryHandlerTests()
    {
        _requestRepositoryMock = new Mock<ILeaveRequestRepository>();
        _loggerMock = new Mock<ILogger<GetLeaveRequestsQueryHandler>>();
        _handler = new GetLeaveRequestsQueryHandler(_requestRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnRequests()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var query = new GetLeaveRequestsQuery { EmployeeId = employeeId };
        var requests = new List<LeaveRequest>
        {
            new() { Id = Guid.NewGuid(), EmployeeId = employeeId, LeaveType = LeaveType.Annual, Status = LeaveRequestStatus.Approved },
            new() { Id = Guid.NewGuid(), EmployeeId = employeeId, LeaveType = LeaveType.Sick, Status = LeaveRequestStatus.Pending }
        };

        _requestRepositoryMock.Setup(r => r.GetByEmployeeIdAsync(employeeId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requests);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _requestRepositoryMock.Verify(r => r.GetByEmployeeIdAsync(employeeId, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}