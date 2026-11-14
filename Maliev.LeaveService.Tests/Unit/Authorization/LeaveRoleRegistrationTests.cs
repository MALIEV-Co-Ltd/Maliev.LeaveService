using Maliev.LeaveService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Maliev.LeaveService.Tests.Unit.Authorization;

public sealed class LeaveRoleRegistrationTests
{
    [Fact]
    public void PublishPermissions_HasUniqueExplicitLeaveActions()
    {
        var registration = CreateRegistration();
        var permissions = registration.GetPermissionsForPublish().ToArray();
        var expected = new[]
        {
            "leave.admin.manage",
            "leave.balances.read",
            "leave.reports.view",
            "leave.requests.approve",
            "leave.requests.cancel",
            "leave.requests.create",
            "leave.requests.read"
        };

        Assert.Equal("leave", registration.ServiceName);
        Assert.Equal(expected, permissions.Select(permission => permission.PermissionId).OrderBy(id => id, StringComparer.Ordinal));
        Assert.Equal(permissions.Length, permissions.Select(permission => permission.PermissionId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(permissions, permission =>
        {
            Assert.DoesNotContain("*", permission.PermissionId, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(permission.Description));
        });
    }

    [Fact]
    public void PublishRoles_DoesNotGrantPredefinedRoles()
    {
        Assert.Empty(CreateRegistration().GetRolesForPublish());
    }

    private static LeaveIAMRegistrationService CreateRegistration() =>
        new(new ConfigurationBuilder().Build(), NullLogger<LeaveIAMRegistrationService>.Instance);
}
