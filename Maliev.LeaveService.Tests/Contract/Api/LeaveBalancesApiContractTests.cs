using System.Net;
using System.Net.Http.Headers;
using Maliev.LeaveService.Tests.TestUtilities;
using Xunit;

namespace Maliev.LeaveService.Tests.Contract.Api;

[Collection("IntegrationTests")]
public class LeaveBalancesApiContractTests
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeaveBalancesApiContractTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.CreateTestToken());
    }

    [Fact]
    public async Task GetBalances_ReturnsOk()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/leave/v1/LeaveBalances/{employeeId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
