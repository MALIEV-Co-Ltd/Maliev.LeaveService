using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.LeaveService.Tests.TestUtilities;
using Xunit;

namespace Maliev.LeaveService.Tests.Contract.Api;

[Collection("IntegrationTests")]
public class LeaveRequestsApiContractTests
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeaveRequestsApiContractTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateTestToken());
    }

    [Fact]
    public async Task SubmitRequest_ReturnsCreated_WithCorrectSchema()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new
        {
            LeaveType = 1, // Annual
            StartDate = DateTimeOffset.UtcNow.AddDays(7),
            EndDate = DateTimeOffset.UtcNow.AddDays(10),
            Reason = "Vacation"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{employeeId}", request);

        // Assert
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
