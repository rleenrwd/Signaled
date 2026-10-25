using Signaled.Application.Events;
using Signaled.Domain.Events;

namespace Signaled.Application.Tests.TestDoubles;

public class SpyEventDraftStore : IEventDraftStore
{
    public Event? SavedEvent { get; private set; }

    public int CallCount { get; private set; }

    public Task SaveDraftAsync(
        Event eventRecord,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SavedEvent = eventRecord;
        CallCount++;

        return Task.CompletedTask;
    }
}