using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Moq;

namespace Maliev.LeaveService.Tests.Unit.Data;

public class DefaultLeavePolicySeederTests
{
    [Fact]
    public async Task SeedAsync_WhenPoliciesAreMissing_AddsDefaultPolicies()
    {
        var repository = new Mock<ILeavePolicyRepository>();
        repository
            .Setup(repo => repo.GetByTypeAsync(It.IsAny<LeaveType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeavePolicy?)null);

        var inserted = await DefaultLeavePolicySeeder.SeedAsync(
            repository.Object,
            Mock.Of<ILogger>(),
            CancellationToken.None);

        Assert.Equal(8, inserted);
        repository.Verify(
            repo => repo.AddAsync(It.Is<LeavePolicy>(policy =>
                policy.LeaveType == LeaveType.Annual &&
                policy.DefaultEntitlement == 20m &&
                policy.IsActive), It.IsAny<CancellationToken>()),
            Times.Once);
        repository.Verify(
            repo => repo.AddAsync(It.Is<LeavePolicy>(policy =>
                policy.LeaveType == LeaveType.Sick &&
                policy.DefaultEntitlement == 30m &&
                policy.IsActive), It.IsAny<CancellationToken>()),
            Times.Once);
        repository.Verify(
            repo => repo.AddAsync(It.Is<LeavePolicy>(policy =>
                policy.LeaveType == LeaveType.Unpaid &&
                policy.DefaultEntitlement == 30m &&
                policy.IsActive), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SeedAsync_WhenPoliciesAlreadyExist_DoesNotOverwriteThem()
    {
        var repository = new Mock<ILeavePolicyRepository>();
        repository
            .Setup(repo => repo.GetByTypeAsync(It.IsAny<LeaveType>(), It.IsAny<CancellationToken>()))
            .Returns((LeaveType leaveType, CancellationToken _) =>
                Task.FromResult<LeavePolicy?>(new LeavePolicy
                {
                    Id = Guid.NewGuid(),
                    LeaveType = leaveType,
                    DefaultEntitlement = 1m,
                    IsActive = false
                }));

        var inserted = await DefaultLeavePolicySeeder.SeedAsync(
            repository.Object,
            Mock.Of<ILogger>(),
            CancellationToken.None);

        Assert.Equal(0, inserted);
        repository.Verify(
            repo => repo.AddAsync(It.IsAny<LeavePolicy>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
