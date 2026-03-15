using Maliev.LeaveService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.LeaveService.Infrastructure.Data.Configurations;

public class LeaveApprovalConfiguration : IEntityTypeConfiguration<LeaveApproval>
{
    public void Configure(EntityTypeBuilder<LeaveApproval> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.ApproverId)
            .HasDatabaseName("ix_leave_approvals_approver_id");
    }
}
