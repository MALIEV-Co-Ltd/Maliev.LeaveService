using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Application.Interfaces;

public interface ILeavePolicyRepository
{
    Task<LeavePolicy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LeavePolicy?> GetByTypeAsync(LeaveType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeavePolicy>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(LeavePolicy policy, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeavePolicy policy, CancellationToken cancellationToken = default);
}
