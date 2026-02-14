using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using MassTransit;
using MassTransit.Testing;
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
        var @event = new Maliev.MessagingContracts.Generated.EmployeeCreatedEvent(
            Guid.NewGuid(),
            nameof(Maliev.MessagingContracts.Generated.EmployeeCreatedEvent),
            Maliev.MessagingContracts.Generated.MessageType.Event,
            "1.0",
            "EmployeeService",
            new[] { "LeaveService" },
            Guid.NewGuid(),
            null,
            DateTimeOffset.UtcNow,
            false,
            new Maliev.MessagingContracts.Generated.EmployeeCreatedEventPayload(
                employeeId,
                "EMP001",
                Guid.NewGuid(), // PrincipalId
                "test@example.com", // Email
                "Test Employee",    // FullName
                DateTimeOffset.UtcNow,
                Guid.NewGuid(), // DepartmentId
                null,           // PositionId
                null)           // ManagerId
        );

        using var scope = _factory.Services.CreateScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        
        // Ensure some policies exist
        var seeder = new DatabaseSeeder(context);
        await seeder.SeedAsync();

        // Act
        await harness.Start();
        await harness.Bus.Publish(@event);
        
        // Wait for consumer
        Assert.True(await harness.Consumed.Any<Maliev.MessagingContracts.Generated.EmployeeCreatedEvent>());

        // Assert
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        var balances = verifyContext.LeaveBalances.Where(b => b.EmployeeId == employeeId).ToList();
        Assert.NotEmpty(balances);
        Assert.Contains(balances, b => b.LeaveType == LeaveType.Annual);
    }
}