using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Signaled.Domain.Identity;

namespace Signaled.Infrastructure.Persistence;

public sealed class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Code).IsRequired().HasMaxLength(100);

        builder.Property(role => role.Name).IsRequired().HasMaxLength(200);

        builder.HasIndex(role => role.Code).IsUnique();
    }
}