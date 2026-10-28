namespace Signaled.Application.Authorization;

public interface ICurrentUserAccess
{
    Task<Guid> RequirePermissionAsync(string permission, CancellationToken cancellationToken = default);
}