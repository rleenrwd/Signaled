using Microsoft.EntityFrameworkCore;
using Signaled.Infrastructure.Persistence;
using Xunit;

namespace Signaled.Infrastructure.Tests;
// CONTEXT IS AN OBJECT USED TO WORK WITH THE DATABASE (TO QUERY, TRACK CHANGES, SAVE CHANGES)
// A FIXTURE IS JUST A CLASS THAT PREPARES A TEST DATABASE, GIVES THE TESTS ACCESS TO IT, AND CLEANS IT UP AFTERWORD.
public class SqlServerFixture : IAsyncLifetime // Provides asynchronous test database setup and cleanup through xUnit.
{
    // Generates one unique db name per fixture instance. All contexts created by this fixture use this same connection string.
    private readonly string _connectionString =
        $@"Server=localhost\SQLEXPRESS;Database=SignaledTests_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True";

    public SignaledDbContext CreateContext()
    {
        // Builds options that configure our context to use the test db
        var options = new DbContextOptionsBuilder<SignaledDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        return new SignaledDbContext(options); // Creates a new context using those options
    }

    public async Task InitializeAsync()
    {
        await using var context = CreateContext(); // Disposes the context when the method exits

        await context.Database.MigrateAsync(); // Creates the test db and applies our migrations
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();

        await context.Database.EnsureDeletedAsync(); // Deletes this fixture's test db after its tests finish
    }
}