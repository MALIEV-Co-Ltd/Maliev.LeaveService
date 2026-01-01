using Moq;
using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Application.Queries.Handlers;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Handlers;

public class GetUtilizationReportQueryHandlerTests
{
    private readonly Mock<ILeaveRequestRepository> _requestRepositoryMock;
    private readonly GetUtilizationReportQueryHandler _handler;

    public GetUtilizationReportQueryHandlerTests()
    {
        _requestRepositoryMock = new Mock<ILeaveRequestRepository>();
        _handler = new GetUtilizationReportQueryHandler(_requestRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnReport()
    {
        // Arrange
        var query = new GetUtilizationReportQuery();
        _requestRepositoryMock.Setup(r => r.GetByEmployeeIdAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Maliev.LeaveService.Domain.Entities.LeaveRequest>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }
}