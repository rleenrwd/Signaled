using Signaled.Application.Events;
using Signaled.Domain.Events;

namespace Signaled.Infrastructure.Persistence;

public class EventDraftStore : IEventDraftStore
{
    private readonly SignaledDbContext _context;

    public EventDraftStore(SignaledDbContext context)
    {
        _context = context;
    }

    public async Task SaveDraftAsync(Event eventRecord, CancellationToken cancellationToken = default)
    {
        _context.Events.Add(eventRecord);

        await _context.SaveChangesAsync(cancellationToken);
    }
}   