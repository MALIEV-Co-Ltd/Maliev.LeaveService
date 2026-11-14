using System.Net;
using System.Text.Json;
using Maliev.LeaveService.Tests.TestUtilities;

namespace Maliev.LeaveService.Tests.Integration;

[Collection("IntegrationTests")]
public sealed class LeaveDocumentationContractTests(TestWebApplicationFactory factory)
{
    [Fact]
    public async Task NonProductionDocumentDescribesLeaveServiceEndpoints()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/leave/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;
        Assert.Equal("MALIEV Leave Service API", root.GetProperty("info").GetProperty("title").GetString());
        Assert.Contains(root.GetProperty("paths").EnumerateObject(),
            path => path.Name.Contains("/leave/", StringComparison.OrdinalIgnoreCase));
    }
}
