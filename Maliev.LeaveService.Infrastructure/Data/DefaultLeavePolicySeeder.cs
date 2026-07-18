using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Infrastructure.Data;

/// <summary>
/// Seeds baseline leave policies required for employee leave request workflows.
/// </summary>
public static class DefaultLeavePolicySeeder
{
    private static readonly DefaultLeavePolicy[] Defaults =
    [
        new(LeaveType.Annual, 20m, 1.67m, 5m, 30, 1),
        new(LeaveType.Sick, 30m, 0m, 0m, 30, 0),
        new(LeaveType.Personal, 3m, 0m, 0m, 5, 0),
        new(LeaveType.Maternity, 98m, 0m, 0m, 98, 1),
        new(LeaveType.Paternity, 15m, 0m, 0m, 15, 1),
        new(LeaveType.Unpaid, 30m, 0m, 0m, 30, 1),
        new(LeaveType.Bereavement, 3m, 0m, 0m, 5, 0),
        new(LeaveType.Study, 10m, 0m, 0m, 10, 1)
    ];

    /// <summary>
    /// Ensures the service has active starter leave policies without overwriting existing policies.
    /// </summary>
    /// <param name="repository">The leave policy repository.</param>
    /// <param name="logger">The logger for seed activity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of policies inserted.</returns>
    public static async Task<int> SeedAsync(
        ILeavePolicyRepository repository,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var inserted = 0;

        foreach (var policy in CreateDefaultPolicies())
        {
            var existing = await repository.GetByTypeAsync(policy.LeaveType, cancellationToken);
            if (existing is not null)
            {
                continue;
            }

            await repository.AddAsync(policy, cancellationToken);
            inserted++;
        }

        if (inserted > 0)
        {
            logger.LogInformation("Seeded {PolicyCount} default leave policies.", inserted);
        }

        return inserted;
    }

    /// <summary>
    /// Creates the baseline leave policy set.
    /// </summary>
    /// <returns>Default leave policies.</returns>
    public static IReadOnlyCollection<LeavePolicy> CreateDefaultPolicies() =>
        Defaults.Select(policy => new LeavePolicy
        {
            Id = Guid.NewGuid(),
            LeaveType = policy.LeaveType,
            DefaultEntitlement = policy.DefaultEntitlement,
            AccrualRate = policy.AccrualRate,
            MaxCarryForward = policy.MaxCarryForward,
            MaxConsecutiveDays = policy.MaxConsecutiveDays,
            RequiredApprovalLevels = policy.RequiredApprovalLevels,
            IsActive = true
        }).ToArray();

    private sealed record DefaultLeavePolicy(
        LeaveType LeaveType,
        decimal DefaultEntitlement,
        decimal AccrualRate,
        decimal MaxCarryForward,
        int MaxConsecutiveDays,
        int RequiredApprovalLevels);
}
