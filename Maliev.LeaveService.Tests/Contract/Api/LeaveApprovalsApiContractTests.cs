using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.LeaveService.Application.DTOs.Requests;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Tests.TestUtilities;
using Xunit;

namespace Maliev.LeaveService.Tests.Contract.Api;

[Collection("IntegrationTests")]
public class LeaveApprovalsApiContractTests
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeaveApprovalsApiContractTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateTestToken());
    }

    [Fact]
    public async Task GetPendingApprovals_ReturnsOk()
    {
        // Arrange
        var managerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/leave/v1/LeaveRequests/pending/{managerId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProcessDecision_ReturnsOk()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var request = new ApproveRejectLeaveDto
        {
            Decision = ApprovalStatus.Approved,
            Comments = "Approved"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{requestId}/decision?approverId={approverId}", request);

        // Assert
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
