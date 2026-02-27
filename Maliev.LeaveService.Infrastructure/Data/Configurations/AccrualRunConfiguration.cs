using Maliev.LeaveService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.LeaveService.Infrastructure.Data.Configurations;

public class AccrualRunConfiguration : IEntityTypeConfiguration<AccrualRun>
{
    public void Configure(EntityTypeBuilder<AccrualRun> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.Year, e.Month }).IsUnique();
    }
}
