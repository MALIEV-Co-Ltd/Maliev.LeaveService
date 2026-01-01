using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.LeaveService.Infrastructure.Repositories;

public class LeaveApprovalRepository : ILeaveApprovalRepository
{
    private readonly LeaveDbContext _context;

    public LeaveApprovalRepository(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaveApproval>> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveApprovals
            .Where(a => a.LeaveRequestId == requestId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(LeaveApproval approval, CancellationToken cancellationToken = default)
    {
        await _context.LeaveApprovals.AddAsync(approval, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}