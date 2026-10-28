namespace Signaled.Domain.Identity;

public sealed class UserPermission
{
    public Guid UserId { get; private set; }

    public Guid PermissionId { get; private set; }

    public DateTimeOffset GrantedAt { get; private set; }

    public Guid? GrantedByUserId { get; private set; }

    private UserPermission()
    {
    }

    public static UserPermission Create(
        Guid userId,
        Guid permissionId,
        DateTimeOffset grantedAt,
        Guid? grantedByUserId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "A user ID is required.",
                nameof(userId));
        }

        if (permissionId == Guid.Empty)
        {
            throw new ArgumentException(
                "A permission ID is required.",
                nameof(permissionId));
        }

        if (grantedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "The granting user ID cannot be empty.",
                nameof(grantedByUserId));
        }

        return new UserPermission
        {
            UserId = userId,
            PermissionId = permissionId,
            GrantedAt = grantedAt.ToUniversalTime(),
            GrantedByUserId = grantedByUserId
        };
    }
}