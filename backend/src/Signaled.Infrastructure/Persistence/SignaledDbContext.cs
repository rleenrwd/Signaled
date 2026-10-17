using Microsoft.EntityFrameworkCore;
using Signaled.Domain.Events;
using Signaled.Infrastructure.Persistence.Configurations;

namespace Signaled.Infrastructure.Persistence;

public class SignaledDbContext : DbContext
{
    public SignaledDbContext(DbContextOptions<SignaledDbContext> options) : base(options)
    {
        
    }

    public DbSet<Event> Events {get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new EventEntityTypeConfiguration().Configure(modelBuilder.Entity<Event>());
    }  
}