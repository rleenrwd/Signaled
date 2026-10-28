using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Signaled.Domain.Identity;

namespace Signaled.Infrastructure.Persistence;

public sealed class UserPermissionEntityTypeConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions");

        builder.HasKey(grant => new
        {
            grant.UserId,
            grant.PermissionId
        });

        builder.HasOne<UserAccount>().WithMany().HasForeignKey(grant => grant.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Permission>().WithMany().HasForeignKey(grant => grant.PermissionId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>().WithMany().HasForeignKey(grant => grant.GrantedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}