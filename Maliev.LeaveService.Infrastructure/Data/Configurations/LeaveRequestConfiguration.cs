using Maliev.LeaveService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.LeaveService.Infrastructure.Data.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TotalDays).HasPrecision(5, 2);

        builder.HasMany(e => e.Approvals)
               .WithOne(e => e.LeaveRequest)
               .HasForeignKey(e => e.LeaveRequestId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
