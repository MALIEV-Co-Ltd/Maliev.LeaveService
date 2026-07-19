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
public class LeaveCancellationTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeaveCancellationTests(TestWebApplicationFactory factory)
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

    private async Task<Guid> SeedRequest(Guid employeeId, LeaveRequestStatus status)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();

        var startDate = DateTimeOffset.UtcNow.AddDays(7);
        var requestYear = startDate.Year;

        var balance = await context.LeaveBalances.FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.Year == requestYear);
        if (balance == null)
        {
            balance = TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Annual, requestYear, 20);
            context.LeaveBalances.Add(balance);
            await context.SaveChangesAsync();
        }

        var request = TestDataBuilder.CreateLeaveRequest(employeeId, LeaveType.Annual, startDate, null, 5, status);
        context.LeaveRequests.Add(request);

        if (status == LeaveRequestStatus.Pending)
            balance.Pending += 5;
        else if (status == LeaveRequestStatus.Approved)
            balance.Used += 5;

        context.LeaveBalances.Update(balance);
        await context.SaveChangesAsync();
        return request.Id;
    }

    [Fact]
    public async Task Employee_CanCancelPendingRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var requestId = await SeedRequest(employeeId, LeaveRequestStatus.Pending);

        // Act
        var response = await _client.PutAsync($"/leave/v1/LeaveRequests/{requestId}/cancel?requestedBy={employeeId}", null);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with {response.StatusCode}: {error}");
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        var updatedRequest = await context.LeaveRequests.FindAsync(requestId);
        var updatedBalance = context.LeaveBalances.First(b => b.EmployeeId == employeeId);

        Assert.Equal(LeaveRequestStatus.Cancelled, updatedRequest?.Status);
        Assert.Equal(0, updatedBalance.Pending);
    }

    [Fact]
    public async Task Employee_CanCancelApprovedRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var requestId = await SeedRequest(employeeId, LeaveRequestStatus.Approved);

        // Act
        var response = await _client.PutAsync($"/leave/v1/LeaveRequests/{requestId}/cancel?requestedBy={employeeId}", null);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with {response.StatusCode}: {error}");
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        var updatedRequest = await context.LeaveRequests.FindAsync(requestId);
        var updatedBalance = context.LeaveBalances.First(b => b.EmployeeId == employeeId);

        Assert.Equal(LeaveRequestStatus.Cancelled, updatedRequest?.Status);
        Assert.Equal(0, updatedBalance.Used);
    }
}
