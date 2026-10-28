namespace Signaled.Domain.Identity;

public sealed class UserAccount
{
    public Guid Id { get; private set; }

    public string IdentityProviderSubject { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public string? Department { get; private set; }

    public Guid? ManagerUserId { get; private set; }

    public UserAccountStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? LastSignInAt { get; private set; }

    private UserAccount()
    {
    }



    public static UserAccount Create(string identityProviderSubject, string displayName, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(identityProviderSubject))
        {
            throw new ArgumentException("An identity provider subject is required.", nameof(identityProviderSubject));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("A display name is required.", nameof(displayName));
        }

        var createdAtUtc = createdAt.ToUniversalTime();

        return new UserAccount
        {
            Id = Guid.NewGuid(),
            IdentityProviderSubject = identityProviderSubject.Trim(),
            DisplayName = displayName.Trim(),
            Status = UserAccountStatus.Active,
            CreatedAt = createdAtUtc,
            UpdatedAt = createdAtUtc
        };
    }

    public void Disable(DateTimeOffset updatedAt)
    {
        Status = UserAccountStatus.Disabled;
        UpdatedAt = updatedAt.ToUniversalTime();
    }















}