using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Application.Interfaces;

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetByEmployeeAndTypeAsync(Guid employeeId, LeaveType type, int year, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveBalance>> GetByEmployeeIdAsync(Guid employeeId, int year, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveBalance>> GetExpiringBalancesAsync(DateTimeOffset expiryDate, CancellationToken cancellationToken = default);
    Task AddAsync(LeaveBalance balance, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaveBalance balance, CancellationToken cancellationToken = default);
}
