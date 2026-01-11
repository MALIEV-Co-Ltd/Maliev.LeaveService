using Maliev.LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.LeaveService.Infrastructure;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Uses environment variable LeaveDbContext for connection string.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LeaveDbContext>
{
    public LeaveDbContext CreateDbContext(string[] args)
    {
        // Prefer environment variable for connection string, fallback to design-time default if not set
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__LeaveDbContext")
            ?? "Host=localhost;Database=leave_design;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<LeaveDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LeaveDbContext(optionsBuilder.Options);
    }
}
