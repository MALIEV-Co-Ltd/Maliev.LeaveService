using Maliev.LeaveService.Application.Interfaces;
using System.Security.Claims;

namespace Maliev.LeaveService.Api.Security;

internal static class LeaveUserAccess
{
    private const string ServiceNameClaim = "service_name";
    private const string UserTypeClaim = "user_type";
    private const string PermissionsClaim = "permissions";
    private const string RolesClaim = "roles";
    private const string AdminRole = "roles.leave.admin";
    private const string AdminPermission = "leave.admin.manage";

    public static bool CanActForEmployee(ClaimsPrincipal user, Guid employeeId)
    {
        if (IsServiceOrAdmin(user))
        {
            return true;
        }

        var currentEmployeeId = GetCurrentEmployeeId(user);
        return currentEmployeeId == employeeId;
    }

    public static async Task<bool> CanActForEmployeeAsync(
        ClaimsPrincipal user,
        Guid employeeId,
        IEmployeeServiceClient employeeServiceClient,
        CancellationToken cancellationToken)
    {
        if (CanActForEmployee(user, employeeId))
        {
            return true;
        }

        var principalId = GetCurrentPrincipalId(user);
        if (principalId is null)
        {
            return false;
        }

        var resolvedEmployeeId = await employeeServiceClient.GetEmployeeIdByPrincipalIdAsync(principalId.Value, cancellationToken);
        return resolvedEmployeeId == employeeId;
    }

    private static bool IsServiceOrAdmin(ClaimsPrincipal user)
    {
        return user.HasClaim(claim => claim.Type == ServiceNameClaim) ||
               user.HasClaim(UserTypeClaim, "service") ||
               user.HasClaim(RolesClaim, AdminRole) ||
               user.HasClaim(ClaimTypes.Role, AdminRole) ||
               user.HasClaim(PermissionsClaim, "*") ||
               user.HasClaim(PermissionsClaim, AdminPermission);
    }

    private static Guid? GetCurrentEmployeeId(ClaimsPrincipal user)
    {
        foreach (var claimType in new[] { "employee_id" })
        {
            var value = user.FindFirst(claimType)?.Value;
            if (Guid.TryParse(value, out var employeeId))
            {
                return employeeId;
            }
        }

        return null;
    }

    private static Guid? GetCurrentPrincipalId(ClaimsPrincipal user)
    {
        foreach (var claimType in new[] { "principal_id", "sub", ClaimTypes.NameIdentifier })
        {
            var value = user.FindFirst(claimType)?.Value;
            if (Guid.TryParse(value, out var principalId))
            {
                return principalId;
            }
        }

        return null;
    }
}
