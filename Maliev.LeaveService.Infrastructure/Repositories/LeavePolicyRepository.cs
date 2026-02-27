using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.LeaveService.Infrastructure.Repositories;

public class LeavePolicyRepository : ILeavePolicyRepository
{
    private readonly LeaveDbContext _context;

    public LeavePolicyRepository(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<LeavePolicy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LeavePolicies.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<LeavePolicy?> GetByTypeAsync(LeaveType type, CancellationToken cancellationToken = default)
    {
        return await _context.LeavePolicies.FirstOrDefaultAsync(p => p.LeaveType == type, cancellationToken);
    }

    public async Task<IEnumerable<LeavePolicy>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LeavePolicies.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(LeavePolicy policy, CancellationToken cancellationToken = default)
    {
        await _context.LeavePolicies.AddAsync(policy, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LeavePolicy policy, CancellationToken cancellationToken = default)
    {
        _context.LeavePolicies.Update(policy);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
