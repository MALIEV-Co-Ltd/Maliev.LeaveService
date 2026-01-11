using Maliev.LeaveService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.LeaveService.Infrastructure.Data;

public class LeaveDbContext : DbContext
{
    public LeaveDbContext(DbContextOptions<LeaveDbContext> options) : base(options)
    {
    }

    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveApproval> LeaveApprovals => Set<LeaveApproval>();
    public DbSet<LeavePolicy> LeavePolicies => Set<LeavePolicy>();
    public DbSet<AccrualRun> AccrualRuns => Set<AccrualRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeaveDbContext).Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Set table name to snake_case
            var tableName = entity.GetTableName();
            if (tableName != null)
            {
                entity.SetTableName(tableName.ToSnakeCase());
            }

            // Set column names to snake_case
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.Name.ToSnakeCase());
            }

            // Set key names to snake_case
            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (keyName != null)
                {
                    key.SetName(keyName.ToSnakeCase());
                }
            }

            // Set foreign key names to snake_case
            foreach (var key in entity.GetForeignKeys())
            {
                var constraintName = key.GetConstraintName();
                if (constraintName != null)
                {
                    key.SetConstraintName(constraintName.ToSnakeCase());
                }
            }

            // Set index names to snake_case
            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (indexName != null)
                {
                    index.SetDatabaseName(indexName.ToSnakeCase());
                }
            }
        }
    }
}
