using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Infrastructure.BackgroundServices;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit;

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyLeaveRequestSubmittedAsync_LogsInfo()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NotificationService>>();
        var httpClient = new HttpClient();
        var service = new NotificationService(httpClient, loggerMock.Object);
        var requestId = Guid.NewGuid();

        // Act
        await service.NotifyLeaveRequestSubmittedAsync(requestId);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Notifying submission for request {requestId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyLeaveRequestDecisionAsync_LogsInfo()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NotificationService>>();
        var httpClient = new HttpClient();
        var service = new NotificationService(httpClient, loggerMock.Object);
        var requestId = Guid.NewGuid();

        // Act
        await service.NotifyLeaveRequestDecisionAsync(requestId);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Notifying decision for request {requestId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyLeaveCancellationAsync_LogsInfo()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NotificationService>>();
        var httpClient = new HttpClient();
        var service = new NotificationService(httpClient, loggerMock.Object);
        var requestId = Guid.NewGuid();

        // Act
        await service.NotifyLeaveCancellationAsync(requestId);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Notifying cancellation for request {requestId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyExpirationAlertAsync_LogsInfo()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NotificationService>>();
        var httpClient = new HttpClient();
        var service = new NotificationService(httpClient, loggerMock.Object);
        var employeeId = Guid.NewGuid();

        // Act
        await service.NotifyExpirationAlertAsync(employeeId, 30);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Notifying expiration alert for employee {employeeId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
