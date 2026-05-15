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
        foreach (var claimType in new[] { "employee_id", "sub", ClaimTypes.NameIdentifier, "principal_id" })
        {
            var value = user.FindFirst(claimType)?.Value;
            if (Guid.TryParse(value, out var employeeId))
            {
                return employeeId;
            }
        }

        return null;
    }
}
