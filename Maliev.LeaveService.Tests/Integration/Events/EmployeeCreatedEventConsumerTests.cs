using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Employee;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration.Events;

[Collection("IntegrationTests")]
public class EmployeeCreatedEventConsumerTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public EmployeeCreatedEventConsumerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Consume_EmployeeCreatedEvent_ShouldInitializeBalances()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var @event = new EmployeeCreatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(EmployeeCreatedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "EmployeeService",
            ConsumedBy: new List<string> { "LeaveService" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new EmployeeCreatedEventPayload(
                EmployeeId: employeeId,
                EmployeeNumber: "EMP001",
                PrincipalId: Guid.NewGuid(),
                Email: "test@example.com",
                FullName: "Test Employee",
                StartDate: DateTimeOffset.UtcNow,
                DepartmentId: Guid.NewGuid(),
                PositionId: null,
                ManagerId: null)
        );

        using var scope = _factory.Services.CreateScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();

        // Ensure some policies exist
        var seeder = new DatabaseSeeder(context);
        await seeder.SeedAsync();

        // Act
        await harness.Bus.Publish(@event);

        // Wait for consumer with generous timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Wait for the message to be processed
        await Task.Delay(2000, cts.Token);

        // Assert - wait a bit for DB update to commit
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<LeaveDbContext>();

        List<LeaveBalance> balances = new();
        // Retry a few times if not yet populated
        for (int i = 0; i < 10 && !balances.Any(); i++)
        {
            await Task.Delay(500, cts.Token);
            balances = await verifyContext.LeaveBalances.Where(b => b.EmployeeId == employeeId).ToListAsync();
        }

        Assert.NotEmpty(balances);
        Assert.Contains(balances, b => b.LeaveType == LeaveType.Annual);
    }
}
