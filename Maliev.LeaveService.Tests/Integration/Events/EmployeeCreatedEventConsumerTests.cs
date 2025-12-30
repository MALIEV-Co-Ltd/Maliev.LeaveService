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
public class EmployeeCreatedEventConsumerTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
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
        var @event = new EmployeeCreatedEvent
        {
            EmployeeId = employeeId,
            EmployeeNumber = "EMP001",
            StartDate = DateTimeOffset.UtcNow,
            DepartmentId = Guid.NewGuid()
        };

        using var scope = _factory.Services.CreateScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        
        // Ensure some policies exist
        var seeder = new DatabaseSeeder(context);
        await seeder.SeedAsync();

        // Act
        await harness.Bus.Publish(@event);
        
        // Wait for consumer
        await harness.Consumed.Any<EmployeeCreatedEvent>();

        // Assert
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        var balances = verifyContext.LeaveBalances.Where(b => b.EmployeeId == employeeId).ToList();
        Assert.NotEmpty(balances);
        Assert.Contains(balances, b => b.LeaveType == LeaveType.Annual);
    }
}