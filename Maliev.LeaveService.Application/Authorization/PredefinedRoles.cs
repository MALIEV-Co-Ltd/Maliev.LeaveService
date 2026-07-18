namespace Maliev.LeaveService.Application.Authorization;

/// <summary>
/// Provides access to predefined roles for the Leave Service.
/// </summary>
public static class LeavePredefinedRoles
{
    public const string Admin = "roles.leave.admin";
    public const string Manager = "roles.leave.manager";
    public const string Employee = "roles.leave.employee";
    public const string Viewer = "roles.leave.viewer";

    public static readonly IReadOnlyList<(string RoleId, string Description, string[] Permissions)> All = new List<(string, string, string[])>
    {
        (
            Admin,
            "Leave Administrator with full access",
            new[]
            {
                LeavePermissions.BalanceRead,
                LeavePermissions.BalanceWrite,
                LeavePermissions.RequestCreate,
                LeavePermissions.RequestRead,
                LeavePermissions.RequestApprove,
                LeavePermissions.RequestReject,
                LeavePermissions.RequestCancel,
                LeavePermissions.PolicyRead,
                LeavePermissions.PolicyManage,
                LeavePermissions.ReportRead,
            }
        ),
        (
            Manager,
            "Leave Manager with approval and report access",
            new[]
            {
                LeavePermissions.BalanceRead,
                LeavePermissions.RequestCreate,
                LeavePermissions.RequestRead,
                LeavePermissions.RequestApprove,
                LeavePermissions.RequestReject,
                LeavePermissions.RequestCancel,
                LeavePermissions.PolicyRead,
                LeavePermissions.ReportRead,
            }
        ),
        (
            Employee,
            "Leave Employee with self-service access",
            new[]
            {
                LeavePermissions.BalanceRead,
                LeavePermissions.RequestCreate,
                LeavePermissions.RequestRead,
                LeavePermissions.RequestCancel,
                LeavePermissions.PolicyRead,
            }
        ),
        (
            Viewer,
            "Leave Viewer with read-only access",
            new[]
            {
                LeavePermissions.BalanceRead,
                LeavePermissions.RequestRead,
                LeavePermissions.PolicyRead,
                LeavePermissions.ReportRead,
            }
        ),
    };
}
