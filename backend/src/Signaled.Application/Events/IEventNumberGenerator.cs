
namespace Signaled.Application.Events;

public interface IEventNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}