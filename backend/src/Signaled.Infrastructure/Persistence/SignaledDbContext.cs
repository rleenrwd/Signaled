using Microsoft.EntityFrameworkCore;
using Signaled.Domain.Events;
using Signaled.Infrastructure.Persistence.Configurations;
using Signaled.Domain.Identity;

namespace Signaled.Infrastructure.Persistence;

public class SignaledDbContext : DbContext
{
    public SignaledDbContext(DbContextOptions<SignaledDbContext> options) : base(options)
    {
        
    }

    public DbSet<Event> Events {get;set;}
    public DbSet<UserAccount> UserAccounts {get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("EventNumberSequence").StartsAt(1).IncrementsBy(1); // A sequence is a db object that allocates numbers - separate from the Events table

        new EventEntityTypeConfiguration().Configure(modelBuilder.Entity<Event>());

        new UserAccountEntityTypeConfiguration().Configure(modelBuilder.Entity<UserAccount>());
    }  
}