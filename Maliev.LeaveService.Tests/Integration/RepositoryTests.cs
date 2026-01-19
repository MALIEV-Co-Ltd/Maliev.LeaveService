using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Maliev.LeaveService.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.LeaveService.Tests.Integration;

[Collection("IntegrationTests")]
public class RepositoryTests : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly IServiceProvider _serviceProvider;

    public RepositoryTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _serviceProvider = factory.Services;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task LeaveApprovalRepository_GetByRequestIdAsync_ReturnsApprovals()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeaveApprovalRepository>();
        var requestRepository = scope.ServiceProvider.GetRequiredService<ILeaveRequestRepository>();

        var leaveRequest = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual,
            StartDate = DateTimeOffset.UtcNow,
            EndDate = DateTimeOffset.UtcNow.AddDays(1),
            TotalDays = 1,
            Status = LeaveRequestStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await requestRepository.AddAsync(leaveRequest);

        var approval = new LeaveApproval
        {
            Id = Guid.NewGuid(),
            LeaveRequestId = leaveRequest.Id,
            ApproverId = Guid.NewGuid(),
            Status = ApprovalStatus.Approved,
            DecidedAt = DateTimeOffset.UtcNow
        };
        await repository.AddAsync(approval);

        // Act
        var results = await repository.GetByRequestIdAsync(leaveRequest.Id);

        // Assert
        Assert.Single(results);
        Assert.Equal(approval.Id, results.First().Id);
    }
}
