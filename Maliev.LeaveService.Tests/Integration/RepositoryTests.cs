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

    [Fact]
    public async Task LeaveBalanceRepository_AddAsync_ShouldAdd()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeaveBalanceRepository>();

        var balance = new LeaveBalance
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual,
            Year = DateTimeOffset.UtcNow.Year,
            Entitled = 20,
            Used = 0,
            Pending = 0,
            CarriedForward = 0
        };

        // Act
        await repository.AddAsync(balance);

        // Assert - verify it was added
        var result = await repository.GetByEmployeeAndTypeAsync(balance.EmployeeId, LeaveType.Annual, DateTimeOffset.UtcNow.Year);
        Assert.NotNull(result);
        Assert.Equal(LeaveType.Annual, result.LeaveType);
    }

    [Fact]
    public async Task LeaveBalanceRepository_GetByEmployeeIdAsync_ShouldReturn()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeaveBalanceRepository>();

        var employeeId = Guid.NewGuid();
        var balance = new LeaveBalance
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveType = LeaveType.Annual,
            Year = DateTimeOffset.UtcNow.Year,
            Entitled = 20,
            Used = 5,
            Pending = 0,
            CarriedForward = 0
        };
        await repository.AddAsync(balance);

        // Act
        var results = await repository.GetByEmployeeIdAsync(employeeId, DateTimeOffset.UtcNow.Year);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal(20, results.First().Entitled);
    }

    [Fact]
    public async Task LeaveBalanceRepository_GetByEmployeeAndTypeAsync_ShouldReturn()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeaveBalanceRepository>();

        var employeeId = Guid.NewGuid();
        var balance = new LeaveBalance
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveType = LeaveType.Annual,
            Year = DateTimeOffset.UtcNow.Year,
            Entitled = 20,
            Used = 5,
            Pending = 0,
            CarriedForward = 0
        };
        await repository.AddAsync(balance);

        // Act
        var result = await repository.GetByEmployeeAndTypeAsync(employeeId, LeaveType.Annual, DateTimeOffset.UtcNow.Year);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(LeaveType.Annual, result.LeaveType);
    }

    [Fact]
    public async Task LeaveBalanceRepository_UpdateAsync_ShouldUpdate()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeaveBalanceRepository>();

        var employeeId = Guid.NewGuid();
        var balance = new LeaveBalance
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveType = LeaveType.Annual,
            Year = DateTimeOffset.UtcNow.Year,
            Entitled = 20,
            Used = 5,
            Pending = 0,
            CarriedForward = 0
        };
        await repository.AddAsync(balance);
        balance.Used = 10;

        // Act
        await repository.UpdateAsync(balance);

        // Assert - fetch again to verify
        var result = await repository.GetByEmployeeAndTypeAsync(employeeId, LeaveType.Annual, DateTimeOffset.UtcNow.Year);
        Assert.NotNull(result);
        Assert.Equal(10, result.Used);
    }

    [Fact]
    public async Task LeaveRequestRepository_AddAsync_ShouldAdd()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeaveRequestRepository>();

        var request = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            LeaveType = LeaveType.Annual,
            StartDate = DateTimeOffset.UtcNow.AddDays(7),
            EndDate = DateTimeOffset.UtcNow.AddDays(10),
            TotalDays = 4,
            Status = LeaveRequestStatus.Pending,
            Reason = "Vacation",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        await repository.AddAsync(request);

        // Assert - verify it was added
        var results = await repository.GetByEmployeeIdAsync(request.EmployeeId);
        Assert.NotNull(results);
        Assert.Single(results);
    }

    [Fact]
    public async Task LeaveRequestRepository_GetByEmployeeIdAsync_ShouldReturn()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeaveRequestRepository>();

        var employeeId = Guid.NewGuid();
        var request = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveType = LeaveType.Annual,
            StartDate = DateTimeOffset.UtcNow.AddDays(7),
            EndDate = DateTimeOffset.UtcNow.AddDays(10),
            TotalDays = 4,
            Status = LeaveRequestStatus.Pending,
            Reason = "Vacation",
            CreatedAt = DateTimeOffset.UtcNow
        };
        await repository.AddAsync(request);

        // Act
        var results = await repository.GetByEmployeeIdAsync(employeeId);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
    }

    [Fact]
    public async Task LeavePolicyRepository_GetAllAsync_ShouldReturn()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeavePolicyRepository>();

        // Act
        var results = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(results);
        Assert.True(results.Count() >= 3);
    }

    [Fact]
    public async Task LeavePolicyRepository_GetByIdAsync_ShouldReturn()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeavePolicyRepository>();

        var policies = await repository.GetAllAsync();
        var policy = policies.First();

        // Act
        var result = await repository.GetByIdAsync(policy.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(policy.Id, result.Id);
    }
}
