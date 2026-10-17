using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Signaled.Domain.Events;

namespace Signaled.Infrastructure.Persistence.Configurations;

public class EventEntityTypeConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.EventNumber).HasMaxLength(32).IsRequired();
        builder.HasIndex(e => e.EventNumber).IsUnique();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(e => e.Title).IsRequired(false);
        builder.Property(e => e.Description).IsRequired(false);
        builder.Property(e => e.OccurredAt).IsRequired(false);

         // Defines a RowVersion column whose value is inserted and updated automatically by SQL Server. Allows EF to Detect/reject concurrency issues.
        builder.Property<byte[]>("RowVersion").IsRowVersion().IsRequired();
    }
}