using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Maliev.LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.LeaveService.Infrastructure.Repositories;

public class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly LeaveDbContext _context;

    public LeaveBalanceRepository(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveBalance?> GetByEmployeeAndTypeAsync(Guid employeeId, LeaveType type, int year, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveBalances
            .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveType == type && b.Year == year, cancellationToken);
    }

    public async Task<IEnumerable<LeaveBalance>> GetByEmployeeIdAsync(Guid employeeId, int year, CancellationToken cancellationToken = default)
    {
        return await _context.LeaveBalances
            .Where(b => b.EmployeeId == employeeId && b.Year == year)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LeaveBalance>> GetExpiringBalancesAsync(DateTimeOffset expiryDate, CancellationToken cancellationToken = default)
    {
        // Simple match on the date part
        var date = expiryDate.Date;
        return await _context.LeaveBalances
            .Where(b => b.ExpirationDate.HasValue && b.ExpirationDate.Value.Date == date && b.CarriedForward > 0)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(LeaveBalance balance, CancellationToken cancellationToken = default)
    {
        await _context.LeaveBalances.AddAsync(balance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LeaveBalance balance, CancellationToken cancellationToken = default)
    {
        _context.LeaveBalances.Update(balance);
        await _context.SaveChangesAsync(cancellationToken);
    }
}