using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Signaled.Domain.Identity;

namespace Signaled.Infrastructure.Persistence;

public sealed class RolePermissionEntityTypeConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(link => new
        {
            link.RoleId,
            link.PermissionId
        });

        builder.HasOne<Role>().WithMany().HasForeignKey(link => link.RoleId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Permission>().WithMany().HasForeignKey(link => link.PermissionId).OnDelete(DeleteBehavior.Restrict);
    }
}