using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Tests.TestUtilities;

public static class TestDataBuilder
{
    public static LeaveRequest CreateLeaveRequest(
        Guid employeeId,
        LeaveType type = LeaveType.Annual,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        decimal days = 1,
        LeaveRequestStatus status = LeaveRequestStatus.Pending)
    {
        var start = startDate ?? DateTimeOffset.UtcNow.AddDays(7);
        var end = endDate ?? start.AddDays((double)days - 1);

        return new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveType = type,
            StartDate = start,
            EndDate = end,
            TotalDays = days,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static LeaveBalance CreateLeaveBalance(
        Guid employeeId,
        LeaveType type = LeaveType.Annual,
        int year = 2025,
        decimal entitled = 20,
        decimal used = 0,
        decimal pending = 0,
        decimal carriedForward = 0)
    {
        return new LeaveBalance
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveType = type,
            Year = year,
            Entitled = entitled,
            Used = used,
            Pending = pending,
            CarriedForward = carriedForward
        };
    }

    public static LeavePolicy CreateLeavePolicy(
        LeaveType type = LeaveType.Annual,
        decimal entitlement = 20,
        decimal accrual = 1.67m,
        int approvalLevels = 1)
    {
        return new LeavePolicy
        {
            Id = Guid.NewGuid(),
            LeaveType = type,
            DefaultEntitlement = entitlement,
            AccrualRate = accrual,
            RequiredApprovalLevels = approvalLevels,
            IsActive = true
        };
    }
}
