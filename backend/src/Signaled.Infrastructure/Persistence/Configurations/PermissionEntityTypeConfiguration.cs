using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Signaled.Domain.Identity;

namespace Signaled.Infrastructure.Persistence;

public sealed class PermissionEntityTypeConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Code).IsRequired().HasMaxLength(150);

        builder.Property(permission => permission.Description).IsRequired().HasMaxLength(1000);

        builder.HasIndex(permission => permission.Code).IsUnique();
    }
}