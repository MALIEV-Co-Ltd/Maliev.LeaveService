using Maliev.LeaveService.Domain.Entities;

namespace Maliev.LeaveService.Application.Interfaces;

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId, int? year = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaveRequest>> GetPendingApprovalsAsync(Guid managerId, CancellationToken cancellationToken = default);
    Task<bool> HasOverlapAsync(Guid employeeId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default);
    Task AddAsync(LeaveRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaveRequest request, CancellationToken cancellationToken = default);
}
