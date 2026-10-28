namespace Signaled.Domain.Identity;

public sealed class Permission
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public PermissionIdentityType AppliesToIdentityType { get; private set; }

    private Permission()
    {
    }

    public static Permission Create(
        Guid id,
        string code,
        string description,
        PermissionIdentityType appliesToIdentityType)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "A permission ID is required.",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A permission code is required.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "A permission description is required.",
                nameof(description));
        }

        if (!Enum.IsDefined(appliesToIdentityType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(appliesToIdentityType),
                appliesToIdentityType,
                "The permission identity type is invalid.");
        }

        return new Permission
        {
            Id = id,
            Code = code.Trim(),
            Description = description.Trim(),
            AppliesToIdentityType = appliesToIdentityType
        };
    }
}