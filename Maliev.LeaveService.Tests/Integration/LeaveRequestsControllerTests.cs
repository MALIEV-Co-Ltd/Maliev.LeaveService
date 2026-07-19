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
public class LeaveRequestsControllerTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeaveRequestsControllerTests(TestWebApplicationFactory factory)
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

    private async Task SeedData(Guid employeeId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();

        var currentYear = DateTimeOffset.UtcNow.Year;

        if (!await context.LeaveBalances.AnyAsync(b => b.EmployeeId == employeeId && b.Year == currentYear))
        {
            context.LeaveBalances.Add(TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Annual, currentYear, 20));
            context.LeaveBalances.Add(TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Sick, currentYear, 30));
        }
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetLeaveRequests_ByEmployee_ReturnsOk()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        await SeedData(employeeId);

        // Act
        var response = await _client.GetAsync($"/leave/v1/LeaveRequests/employee/{employeeId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLeaveRequests_ByEmployee_WithYear_ReturnsOk()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        await SeedData(employeeId);
        var year = DateTimeOffset.UtcNow.Year;

        // Act
        var response = await _client.GetAsync($"/leave/v1/LeaveRequests/employee/{employeeId}?year={year}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingApprovals_ByManager_ReturnsOk()
    {
        // Arrange
        var managerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/leave/v1/LeaveRequests/pending/{managerId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProcessDecision_ForInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var dto = new ApproveRejectLeaveDto { Decision = ApprovalStatus.Approved };

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{requestId}/decision?approverId={approverId}", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CancelRequest_ForInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/leave/v1/LeaveRequests/{requestId}/cancel?requestedBy={requestedBy}", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
