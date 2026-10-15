namespace Signaled.Domain.Events;

public sealed class Event
{
    public Guid Id { get; private set; }

    public string EventNumber { get; private set; } = string.Empty;

    public string? Title { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset? OccurredAt { get; private set; }

    public string? Location { get; private set; }

    public string? Category { get; private set; }

    public string? SourceType { get; private set; }

    public string? SourceSystem { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public Guid? CreatedByServiceIdentityId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public DateTimeOffset? IngestedAt { get; private set; }

    public EventStatus Status { get; private set; }


    private Event() // Private constructor prevents other classes from calling new Event() directly (they can't instantiate an object instance of the Event class directly)
    {
        
    }

    // This is a factory method (CreateDraft) because it creates an object inside and returns it.
    public static Event CreateDraft (Guid createdByUserId, string eventNumber, DateTimeOffset createdAt)
    {
        if (createdByUserId == Guid.Empty) // .Empty is an ALL-ZERO GUID, so we reject it.
        {
            throw new ArgumentException(
                "A creator is required.",
                nameof(createdByUserId)
            );
        }

        if (string.IsNullOrWhiteSpace(eventNumber))
        {
            throw new ArgumentException(
                "An event number is required.",
                nameof(eventNumber)
            );
        }

        return new Event
        {
            Id = Guid.NewGuid(),
            EventNumber = eventNumber.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAt = createdAt.ToUniversalTime(),
            Status = EventStatus.Draft
        };
    }
    

    public void UpdateDraft(string? title, string? description, DateTimeOffset? occurredAt, string? sourceType, string? category, string? location)
    {
        EnsureDraft();

        Title = NormalizeOptionalText(title);
        Description = NormalizeOptionalText(description);
        OccurredAt = occurredAt?.ToUniversalTime();
        SourceType = NormalizeOptionalText(sourceType);
        Category = NormalizeOptionalText(category);
        Location = NormalizeOptionalText(location);
    }

    public void Submit(DateTimeOffset submittedAt)
    {
        EnsureDraft();

        if (string.IsNullOrWhiteSpace(Title))
        {
            throw new InvalidOperationException("An event title is required before submission.");
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            throw new InvalidOperationException("An event description is required before submission.");
        }

        if(OccurredAt is null)
        {
            throw new InvalidOperationException("An occurrence time is required before submission.");
        } 

        SubmittedAt = submittedAt.ToUniversalTime();
        Status = EventStatus.Submitted;
    }



    private void EnsureDraft()
    {
        if (Status != EventStatus.Draft)
        {
            throw new InvalidOperationException("This operation is only allowed for a draft of an event.");
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }






}



