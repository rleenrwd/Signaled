using Signaled.Domain.Identity;
using Xunit;

namespace Signaled.Domain.Tests.Identity;


public class UserAccountTests
{
    [Fact]
    public void Create_WithValidInformation_ReturnsActiveAccount()
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 25, 12, 0, 0, TimeSpan.FromHours(-7)
        );

        // ACT
        var account = UserAccount.Create("  test-identity-subject  ", "    Maya Thompson", createdAt);


        // ASSERT
        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal("test-identity-subject", account.IdentityProviderSubject);
        Assert.Equal("Maya Thompson", account.DisplayName);
        Assert.Equal(UserAccountStatus.Active, account.Status);
        Assert.Equal(createdAt.ToUniversalTime(), account.CreatedAt);
        Assert.Equal(TimeSpan.Zero, account.CreatedAt.Offset);
        Assert.Equal(account.CreatedAt, account.UpdatedAt);
        Assert.Null(account.LastSignInAt);
    }

    [Fact]
    public void Disable_WithActiveAccount_DisablesAndPreservesIdentity()
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

        var account = UserAccount.Create(
            "test-identity-subject",
            "Maya Thompson",
            createdAt);

        var originalId = account.Id;
        var originalSubject = account.IdentityProviderSubject;

        var disabledAt = new DateTimeOffset(
            2026, 10, 26, 12, 0, 0, TimeSpan.FromHours(-7));

        // ACT
        account.Disable(disabledAt);

        // ASSERT
        Assert.Equal(UserAccountStatus.Disabled, account.Status);
        Assert.Equal(disabledAt.ToUniversalTime(), account.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, account.UpdatedAt.Offset);
        Assert.Equal(originalId, account.Id);
        Assert.Equal(originalSubject, account.IdentityProviderSubject);
        Assert.Equal(createdAt, account.CreatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingIdentityProviderSubject_Throws(
        string? identityProviderSubject)
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

        // ACT AND ASSERT
        var exception = Assert.Throws<ArgumentException>(() =>
            UserAccount.Create(
                identityProviderSubject!,
                "Maya Thompson",
                createdAt));

        Assert.Equal("identityProviderSubject", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingDisplayName_Throws(string? displayName)
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

        // ACT AND ASSERT
        var exception = Assert.Throws<ArgumentException>(() =>
            UserAccount.Create(
                "test-identity-subject",
                displayName!,
                createdAt));

        Assert.Equal("displayName", exception.ParamName);
    }
}