namespace Signaled.Domain.Identity;

public sealed class UserRole
{
    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }

    public Guid? AssignedByUserId { get; private set; }

    private UserRole()
    {
    }

    public static UserRole Create(
        Guid userId,
        Guid roleId,
        DateTimeOffset assignedAt,
        Guid? assignedByUserId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "A user ID is required.",
                nameof(userId));
        }

        if (roleId == Guid.Empty)
        {
            throw new ArgumentException(
                "A role ID is required.",
                nameof(roleId));
        }

        if (assignedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "The assigning user ID cannot be empty.",
                nameof(assignedByUserId));
        }

        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = assignedAt.ToUniversalTime(),
            AssignedByUserId = assignedByUserId
        };
    }
}