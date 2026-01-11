using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration;

[Collection("IntegrationTests")]
public class LeaveBalanceViewTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeaveBalanceViewTests(TestWebApplicationFactory factory)
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
    public async Task Employee_CanViewBalances()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
            var year = DateTimeOffset.UtcNow.Year;
            context.LeaveBalances.Add(TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Annual, year, 20, 5, 2, 5));
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/leave/v1/LeaveBalances/{employeeId}?year={DateTimeOffset.UtcNow.Year}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var balances = await response.Content.ReadFromJsonAsync<IEnumerable<LeaveBalanceDto>>(options);
        Assert.NotNull(balances);
        var annual = balances.FirstOrDefault(b => b.LeaveType == LeaveType.Annual);
        Assert.NotNull(annual);
        Assert.Equal(18, annual.Available); // 20 + 5 - 5 - 2 = 18
    }

    [Fact]
    public async Task Employee_CanViewLeaveHistory()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
            context.LeaveRequests.Add(TestDataBuilder.CreateLeaveRequest(employeeId, LeaveType.Annual, DateTimeOffset.UtcNow.AddDays(-10), null, 3, LeaveRequestStatus.Approved));
            context.LeaveRequests.Add(TestDataBuilder.CreateLeaveRequest(employeeId, LeaveType.Sick, DateTimeOffset.UtcNow.AddDays(-5), null, 1, LeaveRequestStatus.Rejected));
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/leave/v1/LeaveRequests/employee/{employeeId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var requests = await response.Content.ReadFromJsonAsync<IEnumerable<LeaveRequestDto>>(options);
        Assert.NotNull(requests);
        Assert.Equal(2, requests.Count());
    }
}