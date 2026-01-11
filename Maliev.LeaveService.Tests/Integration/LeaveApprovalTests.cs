using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.LeaveService.Application.DTOs.Requests;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration;

[Collection("IntegrationTests")]
public class LeaveApprovalTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public LeaveApprovalTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateTestToken());
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> SeedPendingRequest(Guid employeeId)
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

        var request = TestDataBuilder.CreateLeaveRequest(employeeId, LeaveType.Annual, startDate, null, 5);
        context.LeaveRequests.Add(request);
        
        balance.Pending += 5;
        context.LeaveBalances.Update(balance);
        
        await context.SaveChangesAsync();
        return request.Id;
    }

    [Fact]
    public async Task Manager_CanApproveRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var requestId = await SeedPendingRequest(employeeId);
        var managerId = Guid.NewGuid();
        
        var decision = new
        {
            decision = ApprovalStatus.Approved,
            comments = "Approved by Manager"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{requestId}/decision?approverId={managerId}", decision);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify in DB
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        var updatedRequest = await context.LeaveRequests.FindAsync(requestId);
        var updatedBalance = context.LeaveBalances.First(b => b.EmployeeId == employeeId);
        
        Assert.Equal(LeaveRequestStatus.Approved, updatedRequest?.Status);
        Assert.Equal(0, updatedBalance.Pending);
        Assert.Equal(5, updatedBalance.Used);
    }

    [Fact]
    public async Task Manager_CanRejectRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var requestId = await SeedPendingRequest(employeeId);
        var managerId = Guid.NewGuid();
        
        var decision = new
        {
            decision = ApprovalStatus.Rejected,
            comments = "Too many people off"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{requestId}/decision?approverId={managerId}", decision);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify in DB
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        var updatedRequest = await context.LeaveRequests.FindAsync(requestId);
        var updatedBalance = context.LeaveBalances.First(b => b.EmployeeId == employeeId);
        
        Assert.Equal(LeaveRequestStatus.Rejected, updatedRequest?.Status);
        Assert.Equal(0, updatedBalance.Pending);
        Assert.Equal(0, updatedBalance.Used);
    }
}