using Microsoft.EntityFrameworkCore;
using Signaled.Domain.Identity;
using Xunit;

namespace Signaled.Infrastructure.Tests.Identity;

public class UserAccountPersistenceTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public UserAccountPersistenceTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Create_CanBeSavedAndLoaded()
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

        var subject = $"test-subject-{Guid.NewGuid():N}";

        var account = UserAccount.Create(
            subject,
            "Maya Thompson",
            createdAt);

        // ACT
        await using (var writeContext = _fixture.CreateContext())
        {
            writeContext.UserAccounts.Add(account);
            await writeContext.SaveChangesAsync();
        }

       
        await using var readContext = _fixture.CreateContext();

        var savedAccount = await readContext.UserAccounts
            .SingleAsync(user => user.Id == account.Id);

        // Assert
        Assert.Equal(subject, savedAccount.IdentityProviderSubject);
        Assert.Equal("Maya Thompson", savedAccount.DisplayName);
        Assert.Equal(UserAccountStatus.Active, savedAccount.Status);
        Assert.Equal(createdAt, savedAccount.CreatedAt);
        Assert.Equal(createdAt, savedAccount.UpdatedAt);
        Assert.Null(savedAccount.LastSignInAt);
        Assert.Null(savedAccount.ManagerUserId);

        var rowVersion = readContext.Entry(savedAccount)
            .Property<byte[]>("RowVersion")
            .CurrentValue;

        Assert.NotNull(rowVersion);
        Assert.Equal(8, rowVersion.Length);
    }

    [Fact]
    public async Task SaveChanges_WithDuplicateSubject_ThrowsDbUpdateException()
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

        var subject = $"test-subject-{Guid.NewGuid():N}";

        var firstAccount = UserAccount.Create(
            subject,
            "Maya Thompson",
            createdAt);

        var secondAccount = UserAccount.Create(
            subject,
            "Jordan Lee",
            createdAt);

        await using (var firstContext = _fixture.CreateContext())
        {
            firstContext.UserAccounts.Add(firstAccount);
            await firstContext.SaveChangesAsync();
        }

        await using var secondContext = _fixture.CreateContext();

        secondContext.UserAccounts.Add(secondAccount);

        // ACT AND ASSERT
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            secondContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WithStaleRowVersion_RejectsUpdate()
    {
        // ARRANGE: SAVE AN ACTIVE ACCOUNT
        var createdAt = new DateTimeOffset(
            2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

        var account = UserAccount.Create(
            $"test-subject-{Guid.NewGuid():N}",
            "Maya Thompson",
            createdAt);

        await using (var setupContext = _fixture.CreateContext())
        {
            setupContext.UserAccounts.Add(account);
            await setupContext.SaveChangesAsync();
        }

        // LOAD THE SAME ACCOUNT VERSION INTO TWO CONTEXTS
        await using var firstContext = _fixture.CreateContext();
        await using var secondContext = _fixture.CreateContext();

        var firstCopy = await firstContext.UserAccounts
            .SingleAsync(user => user.Id == account.Id);

        var secondCopy = await secondContext.UserAccounts
            .SingleAsync(user => user.Id == account.Id);

        // SAVE THE FIRST UPDATE
        var firstUpdateAt = createdAt.AddHours(1);

        firstCopy.Disable(firstUpdateAt);
        await firstContext.SaveChangesAsync();

        // ATTEMPT AN UPDATE USING THE ORIGINAL ROWVERSION
        secondCopy.Disable(createdAt.AddHours(2));

        // ACT AND ASSERT
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            secondContext.SaveChangesAsync());

        // VERIFY THE FIRST UPDATE REMAINS IN THE DATABASE
        await using var verificationContext = _fixture.CreateContext();

        var savedAccount = await verificationContext.UserAccounts
            .SingleAsync(user => user.Id == account.Id);

        Assert.Equal(UserAccountStatus.Disabled, savedAccount.Status);
        Assert.Equal(firstUpdateAt, savedAccount.UpdatedAt);
    }

    [Fact]
    public async Task SaveChanges_WithMissingManager_ThrowsDbUpdateException()
    {
        // ARRANGE
        var account = UserAccount.Create(
            $"test-subject-{Guid.NewGuid():N}",
            "Jordan Lee",
            new DateTimeOffset(
                2026, 10, 25, 12, 0, 0, TimeSpan.Zero));

        await using var context = _fixture.CreateContext();

        context.UserAccounts.Add(account);

        // SET THE TRACKED FOREIGN KEY TO A NONEXISTENT ACCOUNT
        context.Entry(account)
            .Property(user => user.ManagerUserId)
            .CurrentValue = Guid.NewGuid();

        // ACT AND ASSERT
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync());
    }
}