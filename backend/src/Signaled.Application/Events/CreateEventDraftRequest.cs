namespace Signaled.Application.Events;

public class CreateEventDraftRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset? OccurredAt { get; set; }

    public string? SourceType { get; set; }

    public string? Category { get; set; }

    public string? Location { get; set; }
}