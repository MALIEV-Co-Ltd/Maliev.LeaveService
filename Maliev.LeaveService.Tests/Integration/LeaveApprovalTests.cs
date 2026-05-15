using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Maliev.LeaveService.Application.DTOs.Requests;
using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
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
    public async Task SubmitLeaveRequest_WithApprover_CreatesPendingApprovalVisibleToApprover()
    {
        var employeeId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddDays(7);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
            context.LeavePolicies.Add(TestDataBuilder.CreateLeavePolicy(LeaveType.Annual));
            await context.SaveChangesAsync();
        }

        var submit = new SubmitLeaveRequestDto
        {
            LeaveType = LeaveType.Annual,
            StartDate = start,
            EndDate = start.AddDays(1),
            Reason = "Family appointment",
            ApproverId = approverId
        };

        var submitResponse = await _client.PostAsJsonAsync(
            $"/leave/v1/LeaveRequests/{employeeId}",
            submit,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);
        var submitResult = await submitResponse.Content.ReadFromJsonAsync<JsonElement>();
        var requestId = submitResult.GetProperty("id").GetGuid();

        var pendingResponse = await _client.GetAsync($"/leave/v1/LeaveRequests/pending/{approverId}");
        Assert.Equal(HttpStatusCode.OK, pendingResponse.StatusCode);
        var pending = await pendingResponse.Content.ReadFromJsonAsync<List<LeaveRequestDto>>();
        Assert.NotNull(pending);
        Assert.Contains(pending, request => request.Id == requestId);

        var decision = new
        {
            decision = ApprovalStatus.Approved,
            comments = "Approved by direct manager"
        };
        var decisionResponse = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{requestId}/decision?approverId={approverId}", decision);

        Assert.Equal(HttpStatusCode.OK, decisionResponse.StatusCode);

        var pendingAfterDecision = await _client.GetAsync($"/leave/v1/LeaveRequests/pending/{approverId}");
        Assert.Equal(HttpStatusCode.OK, pendingAfterDecision.StatusCode);
        var remainingPending = await pendingAfterDecision.Content.ReadFromJsonAsync<List<LeaveRequestDto>>();
        Assert.NotNull(remainingPending);
        Assert.DoesNotContain(remainingPending, request => request.Id == requestId);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        var approvedRequest = await verifyContext.LeaveRequests
            .Include(request => request.Approvals)
            .FirstAsync(request => request.Id == requestId);
        var updatedBalance = await verifyContext.LeaveBalances.FirstAsync(balance => balance.EmployeeId == employeeId);

        Assert.Equal(LeaveRequestStatus.Approved, approvedRequest.Status);
        Assert.Contains(approvedRequest.Approvals, approval =>
            approval.ApproverId == approverId && approval.Status == ApprovalStatus.Approved);
        Assert.Equal(0, updatedBalance.Pending);
        Assert.Equal(2, updatedBalance.Used);
    }

    [Fact]
    public async Task SubmitLeaveRequest_UserTokenWithEmployeeIdClaim_AllowsSelfSubmission()
    {
        var employeeId = Guid.NewGuid();
        var principalId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddDays(7);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.CreateTestToken(
                principalId.ToString(),
                roles: [],
                additionalClaims: [new Claim("employee_id", employeeId.ToString())]));

        var submit = new SubmitLeaveRequestDto
        {
            LeaveType = LeaveType.Annual,
            StartDate = start,
            EndDate = start.AddDays(1),
            Reason = "Self-service leave request"
        };

        var submitResponse = await _client.PostAsJsonAsync(
            $"/leave/v1/LeaveRequests/{employeeId}",
            submit,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        var submitBody = await submitResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);
        Assert.Contains("id", submitBody, StringComparison.OrdinalIgnoreCase);
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
