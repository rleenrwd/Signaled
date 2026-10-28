using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Signaled.Domain.Identity;

namespace Signaled.Infrastructure.Persistence;

public sealed class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(assignment => new
        {
            assignment.UserId,
            assignment.RoleId
        });

        builder.HasOne<UserAccount>().WithMany().HasForeignKey(assignment => assignment.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Role>().WithMany().HasForeignKey(assignment => assignment.RoleId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>().WithMany().HasForeignKey(assignment => assignment.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}