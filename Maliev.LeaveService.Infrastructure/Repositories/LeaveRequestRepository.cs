using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.LeaveService.Infrastructure.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly LeaveDbContext _context;

    public LeaveRequestRepository(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .Include(r => r.Approvals)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(Guid employeeId, int? year = null, CancellationToken cancellationToken = default)
    {
        var query = _context.LeaveRequests.Where(r => r.EmployeeId == employeeId);
        if (year.HasValue)
        {
            query = query.Where(r => r.StartDate.Year == year.Value || r.EndDate.Year == year.Value);
        }
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingApprovalsAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        // Simple implementation for now, assuming managerId is linked to approvals
        return await _context.LeaveRequests
            .Where(r => r.Approvals.Any(a => a.ApproverId == managerId && a.Status == Domain.Enums.ApprovalStatus.Pending))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(Guid employeeId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveRequests
            .AnyAsync(r => r.EmployeeId == employeeId &&
                           r.Status != Domain.Enums.LeaveRequestStatus.Rejected &&
                           r.Status != Domain.Enums.LeaveRequestStatus.Cancelled &&
                           r.StartDate <= endDate &&
                           r.EndDate >= startDate, cancellationToken);
    }

    public async Task AddAsync(LeaveRequest request, CancellationToken cancellationToken = default)
    {
        await _context.LeaveRequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LeaveRequest request, CancellationToken cancellationToken = default)
    {
        _context.LeaveRequests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
