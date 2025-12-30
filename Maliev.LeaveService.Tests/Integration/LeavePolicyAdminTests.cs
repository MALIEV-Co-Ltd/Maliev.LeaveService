using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.LeaveService.Application.DTOs.Requests;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration;

[Collection("IntegrationTests")]
public class LeavePolicyAdminTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeavePolicyAdminTests(TestWebApplicationFactory factory)
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

    [Fact]
    public async Task Admin_CanCreatePolicy()
    {
        // Arrange
        var request = new
        {
            leave_type = LeaveType.Bereavement,
            default_entitlement = 5,
            accrual_rate = 0,
            max_carry_forward = 0,
            required_approval_levels = 1,
            max_consecutive_days = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/leave/v1/LeavePolicies", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        Assert.True(context.LeavePolicies.Any(p => p.LeaveType == LeaveType.Bereavement));
    }

    [Fact]
    public async Task Admin_CanUpdatePolicy()
    {
        // Arrange
        Guid policyId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
            var policy = TestDataBuilder.CreateLeavePolicy(LeaveType.Study, 10, 0, 1);
            context.LeavePolicies.Add(policy);
            await context.SaveChangesAsync();
            policyId = policy.Id;
        }

        var updateRequest = new
        {
            default_entitlement = 15,
            is_active = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/leave/v1/LeavePolicies/{policyId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<LeaveDbContext>();
        var updated = await verifyContext.LeavePolicies.FindAsync(policyId);
        Assert.Equal(15, updated?.DefaultEntitlement);
        Assert.False(updated?.IsActive);
    }
}