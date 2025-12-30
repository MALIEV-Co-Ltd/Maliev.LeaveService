namespace Maliev.LeaveService.Domain.Authorization;

public static class LeavePermissions
{
    public const string Create = "leave.requests.create";
    public const string Read = "leave.requests.read";
    public const string Approve = "leave.requests.approve";
    public const string Cancel = "leave.requests.cancel";
    public const string Admin = "leave.admin.manage";
    public const string Reports = "leave.reports.view";
}
