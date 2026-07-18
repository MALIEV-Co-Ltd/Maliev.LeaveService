using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Employee;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration.Events;

[Collection("IntegrationTests")]
public class EmployeeTerminatedEventConsumerTests : IAsyncLifetime
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
        var @event = new EmployeeTerminatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(EmployeeTerminatedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "EmployeeService",
            ConsumedBy: new List<string> { "LeaveService" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new EmployeeTerminatedEventPayload(
                EmployeeId: employeeId,
                TerminationDate: DateTimeOffset.UtcNow,
                TerminationReason: "Test",
                EligibleForRehire: true)
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

        // Wait for consumer with generous timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Wait for the message to be sent to the queue
        await Task.Delay(2000, cts.Token);

        // Assert - check database state directly
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<LeaveDbContext>();

        var updatedRequest = await verifyContext.LeaveRequests.FindAsync(request.Id);
        // Retry a few times if not yet updated
        for (int i = 0; i < 10 && updatedRequest?.Status != LeaveRequestStatus.Cancelled; i++)
        {
            await Task.Delay(500, cts.Token);
            await verifyContext.Entry(request).ReloadAsync();
            updatedRequest = await verifyContext.LeaveRequests.FindAsync(request.Id);
        }

        Assert.Equal(LeaveRequestStatus.Cancelled, updatedRequest?.Status);
    }
}
