using System.Net;
using System.Text;
using System.Text.Json;
using Maliev.LeaveService.Infrastructure.Services;

namespace Maliev.LeaveService.Tests.Unit.Services;

public sealed class EmployeeServiceClientTests
{
    [Theory]
    [InlineData("id")]
    [InlineData("Id")]
    [InlineData("employee_id")]
    [InlineData("employeeId")]
    [InlineData("EmployeeId")]
    public async Task PrincipalLookupAcceptsSupportedEmployeeIdShapes(string field)
    {
        var principalId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var handler = new StubHandler(_ => Json(HttpStatusCode.OK, $"{{\"{field}\":\"{employeeId}\"}}"));
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://employee.invalid/") };

        var result = await new EmployeeServiceClient(http).GetEmployeeIdByPrincipalIdAsync(principalId);

        Assert.Equal(employeeId, result);
        Assert.Equal($"/employee/v1/employees/by-principal/{principalId}", handler.Path);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"id\":\"not-a-guid\"}")]
    [InlineData("{\"id\":123}")]
    public async Task PrincipalLookupRejectsMissingOrInvalidEmployeeIds(string body)
    {
        using var http = new HttpClient(new StubHandler(_ => Json(HttpStatusCode.OK, body)))
        { BaseAddress = new Uri("https://employee.invalid/") };

        var result = await new EmployeeServiceClient(http).GetEmployeeIdByPrincipalIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UnsuccessfulPrincipalLookupReturnsNoEmployeeWithoutParsingErrorBody()
    {
        using var http = new HttpClient(new StubHandler(_ => Json(HttpStatusCode.Forbidden, "not-json")))
        { BaseAddress = new Uri("https://employee.invalid/") };

        var result = await new EmployeeServiceClient(http).GetEmployeeIdByPrincipalIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task MalformedSuccessfulPrincipalResponseDoesNotGrantAccess()
    {
        using var http = new HttpClient(new StubHandler(_ => Json(HttpStatusCode.OK, "not-json")))
        { BaseAddress = new Uri("https://employee.invalid/") };

        await Assert.ThrowsAnyAsync<JsonException>(() =>
            new EmployeeServiceClient(http).GetEmployeeIdByPrincipalIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ActiveEmployeeSearchUsesActiveFilterAndReturnsResultIds()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var handler = new StubHandler(_ => Json(HttpStatusCode.OK,
            $"{{\"results\":[{{\"id\":\"{first}\"}},{{\"id\":\"{second}\"}}]}}"));
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://employee.invalid/") };

        var result = await new EmployeeServiceClient(http).GetActiveEmployeeIdsAsync();

        Assert.Equal(new[] { first, second }, result);
        Assert.Equal("/employee/v1/reports/employees/search?EmploymentStatus=Active&PageSize=1000", handler.Path);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        public string? Path { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Path = request.RequestUri?.PathAndQuery;
            return Task.FromResult(response(request));
        }
    }
}
