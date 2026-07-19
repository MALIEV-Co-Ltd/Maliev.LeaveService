using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration;

[Collection("IntegrationTests")]
public class LeaveRequestSubmissionTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LeaveRequestSubmissionTests(TestWebApplicationFactory factory)
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
        var nextYear = currentYear + 1;

        // Seed balance for the employee for current year
        if (!await context.LeaveBalances.AnyAsync(b => b.EmployeeId == employeeId && b.Year == currentYear))
        {
            context.LeaveBalances.Add(TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Annual, currentYear, 20));
            context.LeaveBalances.Add(TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Sick, currentYear, 30));
        }

        // Seed balance for the employee for next year
        if (!await context.LeaveBalances.AnyAsync(b => b.EmployeeId == employeeId && b.Year == nextYear))
        {
            context.LeaveBalances.Add(TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Annual, nextYear, 20));
            context.LeaveBalances.Add(TestDataBuilder.CreateLeaveBalance(employeeId, LeaveType.Sick, nextYear, 30));
        }
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task SubmitLeaveRequest_WithSufficientBalance_ShouldSucceed()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        await SeedData(employeeId);

        var request = new
        {
            leave_type = LeaveType.Annual,
            start_date = DateTimeOffset.UtcNow.AddDays(7),
            end_date = DateTimeOffset.UtcNow.AddDays(10),
            reason = "Vacation"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{employeeId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task SubmitLeaveRequest_WithInsufficientBalance_ShouldFail()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        await SeedData(employeeId);

        var request = new
        {
            leave_type = LeaveType.Annual,
            start_date = DateTimeOffset.UtcNow.AddDays(7),
            end_date = DateTimeOffset.UtcNow.AddDays(30), // 24 days, but balance is 20
            reason = "Too long"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{employeeId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SubmitLeaveRequest_LessThen24HourNotice_ShouldFail()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        await SeedData(employeeId);

        var request = new
        {
            leave_type = LeaveType.Annual,
            start_date = DateTimeOffset.UtcNow.AddHours(12),
            end_date = DateTimeOffset.UtcNow.AddDays(2),
            reason = "Last minute trip"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{employeeId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SubmitSickLeave_WithSameDayNotice_ShouldSucceed()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        await SeedData(employeeId);

        var request = new
        {
            leave_type = LeaveType.Sick,
            start_date = DateTimeOffset.UtcNow,
            end_date = DateTimeOffset.UtcNow.AddDays(1),
            reason = "Feeling unwell"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/leave/v1/LeaveRequests/{employeeId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
