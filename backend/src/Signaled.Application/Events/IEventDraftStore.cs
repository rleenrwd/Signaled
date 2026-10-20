using Signaled.Domain.Events;

namespace Signaled.Application.Events;

public interface IEventDraftStore
{
    Task SaveDraftAsync(Event eventRecord, CancellationToken cancellationToken = default);
}