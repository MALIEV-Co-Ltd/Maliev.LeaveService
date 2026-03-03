using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Infrastructure.BackgroundServices;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.BackgroundServices;

public class LeaveAccrualBackgroundServiceTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LeaveAccrualBackgroundService>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        // Act
        var service = new LeaveAccrualBackgroundService(serviceProviderMock.Object, loggerMock.Object);

        // Assert
        Assert.NotNull(service);
    }
}
