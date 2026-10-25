using Signaled.Domain.Events;

namespace Signaled.Application.Events;

public class CreateEventDraftService
{
    private readonly IEventNumberGenerator _eventNumberGenerator;
    private readonly IEventDraftStore _eventDraftStore;
    private readonly TimeProvider _timeProvider;


    public CreateEventDraftService(IEventNumberGenerator eventNumberGenerator, IEventDraftStore eventDraftStore, TimeProvider timeProvider)
    {
        _eventNumberGenerator = eventNumberGenerator;
        _eventDraftStore = eventDraftStore;
        _timeProvider = timeProvider;
    }

    public async Task<Event> CreateAsync(CreateEventDraftRequest request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException("A creator is required.", nameof(createdByUserId));
        }

        string eventNumber = await _eventNumberGenerator.GenerateAsync(cancellationToken);

        var eventRecord = Event.CreateDraft(createdByUserId, eventNumber, _timeProvider.GetUtcNow());

        eventRecord.UpdateDraft(
            request.Title,
            request.Description,
            request.OccurredAt,
            request.SourceType,
            request.Category,
            request.Location
        );

        await _eventDraftStore.SaveDraftAsync(eventRecord, cancellationToken);

        return eventRecord;
    }
}