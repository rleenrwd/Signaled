
namespace Signaled.Domain.Events;

public enum EventStatus
{
    Draft = 0,
    Submitted = 1,
    FollowUpRequested = 2,
    EscalationPending = 3,
    ReviewedNoCase = 4,
    CaseLinked = 5

}