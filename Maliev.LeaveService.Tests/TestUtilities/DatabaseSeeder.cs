using Maliev.LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.LeaveService.Tests.TestUtilities;

public class DatabaseSeeder
{
    private readonly LeaveDbContext _context;

    public DatabaseSeeder(LeaveDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Use EnsureCreated for integration tests (no migration files)
        await _context.Database.EnsureCreatedAsync();

        // Add default policies if none exist
        if (!await _context.LeavePolicies.AnyAsync())
        {
            _context.LeavePolicies.Add(TestDataBuilder.CreateLeavePolicy(Domain.Enums.LeaveType.Annual));
            _context.LeavePolicies.Add(TestDataBuilder.CreateLeavePolicy(Domain.Enums.LeaveType.Sick, 30, 0));
            _context.LeavePolicies.Add(TestDataBuilder.CreateLeavePolicy(Domain.Enums.LeaveType.Personal, 3, 0));

            await _context.SaveChangesAsync();
        }
    }
}
