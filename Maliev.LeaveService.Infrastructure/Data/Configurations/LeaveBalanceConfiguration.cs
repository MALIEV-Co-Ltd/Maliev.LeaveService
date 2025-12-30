using Maliev.LeaveService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.LeaveService.Infrastructure.Data.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Entitled).HasPrecision(5, 2);
        builder.Property(e => e.Used).HasPrecision(5, 2);
        builder.Property(e => e.Pending).HasPrecision(5, 2);
        builder.Property(e => e.CarriedForward).HasPrecision(5, 2);

        builder.HasIndex(e => new { e.EmployeeId, e.LeaveType, e.Year }).IsUnique();
    }
}
