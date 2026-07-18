namespace Maliev.LeaveService.Application.Authorization;

/// <summary>
/// Defines the permissions for the Leave Service.
/// </summary>
public static class LeavePermissions
{
    public const string BalanceRead = "leave.balances.read";
    public const string BalanceWrite = "leave.balances.write";

    public const string RequestCreate = "leave.requests.create";
    public const string RequestRead = "leave.requests.read";
    public const string RequestApprove = "leave.requests.approve";
    public const string RequestReject = "leave.requests.reject";
    public const string RequestCancel = "leave.requests.cancel";

    public const string PolicyRead = "leave.policies.read";
    public const string PolicyManage = "leave.policies.manage";

    public const string ReportRead = "leave.reports.read";

    public static readonly IReadOnlyDictionary<string, string> AllWithDescriptions = new Dictionary<string, string>
    {
        { BalanceRead, "Read leave balances" },
        { BalanceWrite, "Write leave balances" },
        { RequestCreate, "Create leave requests" },
        { RequestRead, "Read leave requests" },
        { RequestApprove, "Approve leave requests" },
        { RequestReject, "Reject leave requests" },
        { RequestCancel, "Cancel leave requests" },
        { PolicyRead, "Read leave policies" },
        { PolicyManage, "Manage leave policies" },
        { ReportRead, "Read leave reports" },
    };

    public static string[] All => AllWithDescriptions.Keys.ToArray();
}
