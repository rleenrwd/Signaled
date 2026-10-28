using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Signaled.Domain.Identity;

namespace Signaled.Infrastructure.Persistence;

public sealed class UserAccountEntityTypeConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("UserAccounts"); 

        builder.HasKey(user => user.Id);

        builder.Property(user => user.IdentityProviderSubject).IsRequired().HasMaxLength(200);

        builder.HasIndex(user => user.IdentityProviderSubject).IsUnique();

        builder.Property(user => user.DisplayName).IsRequired().HasMaxLength(200);

        builder.Property(user => user.Email).HasMaxLength(254);

        builder.Property(user => user.Phone).HasMaxLength(50);

        builder.Property(user => user.Department).HasMaxLength(200);

        builder.HasOne<UserAccount>().WithMany().HasForeignKey(user => user.ManagerUserId).OnDelete(DeleteBehavior.Restrict);
        
        builder.Property<byte[]>("RowVersion").IsRowVersion().IsRequired(); // Defines a RowVersion column whose value is inserted and updated automatically by SQL Server. Allows EF to Detect/reject concurrency issues.
    }
}