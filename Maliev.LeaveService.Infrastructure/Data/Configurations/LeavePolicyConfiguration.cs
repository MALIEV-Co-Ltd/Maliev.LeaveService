using Maliev.LeaveService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.LeaveService.Infrastructure.Data.Configurations;

public class LeavePolicyConfiguration : IEntityTypeConfiguration<LeavePolicy>
{
    public void Configure(EntityTypeBuilder<LeavePolicy> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DefaultEntitlement).HasPrecision(5, 2);
        builder.Property(e => e.AccrualRate).HasPrecision(5, 2);
        builder.Property(e => e.MaxCarryForward).HasPrecision(5, 2);

        builder.HasIndex(e => e.LeaveType).IsUnique();
    }
}
