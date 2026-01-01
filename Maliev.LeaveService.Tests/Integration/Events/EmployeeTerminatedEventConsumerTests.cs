using Maliev.LeaveService.Domain.Events.Consumed;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration.Events;

[Collection("IntegrationTests")]
public class EmployeeTerminatedEventConsumerTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public EmployeeTerminatedEventConsumerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Consume_EmployeeTerminatedEvent_ShouldCancelPendingRequests()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var @event = new Maliev.MessagingContracts.Generated.EmployeeTerminatedEvent(
            Guid.NewGuid(),
            "EmployeeTerminatedEvent",
            Maliev.MessagingContracts.Generated.MessageType.Event,
            "1.0",
            "Test",
            new List<string> { "LeaveService" },
            Guid.NewGuid(),
            null,
            DateTimeOffset.UtcNow,
            false,
            new Maliev.MessagingContracts.Generated.EmployeeTerminatedEventPayload(
                employeeId,
                DateTimeOffset.UtcNow,
                "Test",
                true)
        );

        using var scope = _factory.Services.CreateScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        
        // Seed a pending request
        var request = TestDataBuilder.CreateLeaveRequest(employeeId, LeaveType.Annual, DateTimeOffset.UtcNow.AddDays(7), null, 5, LeaveRequestStatus.Pending);
        context.LeaveRequests.Add(request);
        await context.SaveChangesAsync();

        // Act
        await harness.Bus.Publish(@event);
        
        // Wait for consumer
        await harness.Consumed.Any<Maliev.MessagingContracts.Generated.EmployeeTerminatedEvent>();

        // Assert
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        var updatedRequest = await verifyContext.LeaveRequests.FindAsync(request.Id);
        Assert.Equal(LeaveRequestStatus.Cancelled, updatedRequest?.Status);
    }
}