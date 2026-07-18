using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Maliev.LeaveService.Application.DTOs.Requests;
using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Tests.TestUtilities;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration;

[Collection("IntegrationTests")]
public class AdditionalApiTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AdditionalApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateTestToken());
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetLeaveTypes_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/leave/v1/LeaveTypes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var types = await response.Content.ReadFromJsonAsync<IEnumerable<LeavePolicyDto>>();
        Assert.NotNull(types);
    }

    [Fact]
    public async Task GetUtilizationReport_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/leave/v1/Reports/utilization?year=2024");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await response.Content.ReadFromJsonAsync<LeaveUtilizationReportDto>();
        Assert.NotNull(report);
        Assert.Equal(2024, report.Year);
    }

    [Fact]
    public async Task CreateLeavePolicy_WhenInvalid_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateLeavePolicyDto { DefaultEntitlement = -1 }; // Invalid

        // Act
        var response = await _client.PostAsJsonAsync("/leave/v1/LeavePolicies", dto, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLeavePolicy_WhenNotFound_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateLeavePolicyDto { DefaultEntitlement = 20 };

        // Act
        var response = await _client.PutAsJsonAsync($"/leave/v1/LeavePolicies/{id}", dto, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SubmitLeaveRequest_WhenInvalid_ReturnsBadRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var dto = new SubmitLeaveRequestDto { Reason = "" }; // Missing dates

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{employeeId}", dto, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProcessDecision_WhenNotFound_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var dto = new ApproveRejectLeaveDto { Decision = ApprovalStatus.Approved };

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{requestId}/decision?approverId={approverId}", dto, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cancel_WhenNotFound_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/leave/v1/LeaveRequests/{requestId}/cancel?requestedBy={requestedBy}", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetLeavePolicies_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/leave/v1/LeavePolicies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
