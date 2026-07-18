using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration;

[Collection("IntegrationTests")]
public class LeaveBalancesControllerTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeaveBalancesControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateTestToken());
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetBalances_WithValidEmployee_ReturnsOk()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        // Seed balance
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        var currentYear = DateTimeOffset.UtcNow.Year;

        context.LeaveBalances.Add(TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Annual, currentYear, 20));
        context.LeaveBalances.Add(TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Sick, currentYear, 30));
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/leave/v1/LeaveBalances/{employeeId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var balances = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(balances);
        Assert.Equal(2, balances.Count);
    }

    [Fact]
    public async Task GetBalances_WithYearFilter_ReturnsOk()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var year = DateTimeOffset.UtcNow.Year;

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();

        context.LeaveBalances.Add(TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Annual, year, 20));
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/leave/v1/LeaveBalances/{employeeId}?year={year}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBalances_WithNoBalance_ReturnsEmpty()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/leave/v1/LeaveBalances/{employeeId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var balances = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(balances);
        Assert.Empty(balances);
    }
}
