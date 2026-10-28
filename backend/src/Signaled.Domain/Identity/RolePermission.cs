namespace Signaled.Domain.Identity;

public sealed class RolePermission
{
    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    private RolePermission()
    {
    }

    public static RolePermission Create(
        Guid roleId,
        Guid permissionId)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException(
                "A role ID is required.",
                nameof(roleId));
        }

        if (permissionId == Guid.Empty)
        {
            throw new ArgumentException(
                "A permission ID is required.",
                nameof(permissionId));
        }

        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
    }
}