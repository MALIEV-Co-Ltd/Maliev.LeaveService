using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.LeaveService.Application.DTOs.Requests;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Tests.TestUtilities;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration;

[Collection("IntegrationTests")]
public class LeaveResourceAuthorizationTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public LeaveResourceAuthorizationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetBalances_DifferentEmployee_ReturnsForbidden()
    {
        var actorEmployeeId = Guid.NewGuid();
        var targetEmployeeId = Guid.NewGuid();
        using var client = CreateEmployeeClient(actorEmployeeId);

        var response = await client.GetAsync($"/leave/v1/LeaveBalances/{targetEmployeeId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_MatchingEmployee_ReturnsOk()
    {
        var actorEmployeeId = Guid.NewGuid();
        using var client = CreateEmployeeClient(actorEmployeeId);

        var response = await client.GetAsync($"/leave/v1/LeaveRequests/employee/{actorEmployeeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingApprovals_DifferentManager_ReturnsForbidden()
    {
        var actorEmployeeId = Guid.NewGuid();
        var targetManagerId = Guid.NewGuid();
        using var client = CreateEmployeeClient(actorEmployeeId);

        var response = await client.GetAsync($"/leave/v1/LeaveRequests/pending/{targetManagerId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ProcessDecision_DifferentApprover_ReturnsForbidden()
    {
        var actorEmployeeId = Guid.NewGuid();
        var targetApproverId = Guid.NewGuid();
        using var client = CreateEmployeeClient(actorEmployeeId);
        var decision = new ApproveRejectLeaveDto
        {
            Decision = ApprovalStatus.Approved,
            Comments = "approved"
        };

        var response = await client.PostAsJsonAsync(
            $"/leave/v1/LeaveRequests/{Guid.NewGuid()}/decision?approverId={targetApproverId}",
            decision);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_DifferentRequestedBy_ReturnsForbidden()
    {
        var actorEmployeeId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();
        using var client = CreateEmployeeClient(actorEmployeeId);

        var response = await client.PutAsync(
            $"/leave/v1/LeaveRequests/{Guid.NewGuid()}/cancel?requestedBy={requestedBy}",
            null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private HttpClient CreateEmployeeClient(Guid employeeId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.CreateTestToken(employeeId.ToString(), []));
        return client;
    }
}
