using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.LeaveService.Domain.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Infrastructure.Services;

/// <summary>
/// Service to register leave management permissions with the IAM service on startup.
/// </summary>
public class LeaveIAMRegistrationService : IAMRegistrationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LeaveIAMRegistrationService"/> class.
    /// </summary>
    public LeaveIAMRegistrationService(IConfiguration configuration, ILogger<LeaveIAMRegistrationService> logger)
        : base(configuration, logger, "leave")
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return new[]
        {
            new PermissionRegistration { PermissionId = LeavePermissions.Create, Description = "Create leave requests" },
            new PermissionRegistration { PermissionId = LeavePermissions.Read, Description = "Read leave requests and balances" },
            new PermissionRegistration { PermissionId = LeavePermissions.Approve, Description = "Approve or reject leave requests" },
            new PermissionRegistration { PermissionId = LeavePermissions.Cancel, Description = "Cancel leave requests" },
            new PermissionRegistration { PermissionId = LeavePermissions.Admin, Description = "Administrative access to leave management" },
            new PermissionRegistration { PermissionId = LeavePermissions.Reports, Description = "Generate leave and accrual reports" }
        };
    }

    /// <inheritdoc/>
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return Enumerable.Empty<RoleRegistration>();
    }
}
