using Maliev.LeaveService.Domain.Entities;

namespace Maliev.LeaveService.Application.Interfaces;

public interface ILeaveApprovalRepository
{
    Task<IEnumerable<LeaveApproval>> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task AddAsync(LeaveApproval approval, CancellationToken cancellationToken = default);
}
