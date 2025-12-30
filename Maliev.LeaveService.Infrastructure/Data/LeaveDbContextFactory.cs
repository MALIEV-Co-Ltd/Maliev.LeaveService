using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.LeaveService.Infrastructure.Data;

public class LeaveDbContextFactory : IDesignTimeDbContextFactory<LeaveDbContext>
{
    public LeaveDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LeaveDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=leave_db;Username=postgres;Password=postgres");

        return new LeaveDbContext(optionsBuilder.Options);
    }
}
