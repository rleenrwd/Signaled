using Signaled.Domain.Events;

namespace Signaled.Domain.Tests.Events;

public class EventTests
{
    [Fact]
    public void CreateDraft_WithValidCreator_CreatesIncompleteDraft()
    {
        // ARRANGE
        var creatorId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 10, 15, 5, 0, 0, TimeSpan.Zero);

        // ACT
        var eventRecord = Event.CreateDraft(creatorId, "EVT-1045", createdAt);

        //ASSERT
        Assert.NotEqual(Guid.Empty, eventRecord.Id);
        Assert.Equal("EVT-1045", eventRecord.EventNumber);
        Assert.Equal(creatorId, eventRecord.CreatedByUserId);
        Assert.Equal(createdAt, eventRecord.CreatedAt);
        Assert.Equal(EventStatus.Draft, eventRecord.Status);
        Assert.Null(eventRecord.Title);
        Assert.Null(eventRecord.Description);
        Assert.Null(eventRecord.OccurredAt);
        Assert.Null(eventRecord.SubmittedAt);
        Assert.Null(eventRecord.CreatedByServiceIdentityId);
    }

    [Fact]
    public void CreateDraft_WithEmptyCreator_ThrowsArgumentException()
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(2026, 10, 15, 5, 0, 0, TimeSpan.Zero);

        // ACT AND ASSERT
        Assert.Throws<ArgumentException>(() =>
            Event.CreateDraft(
                Guid.Empty,
                "EVT-1046",
                createdAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateDraft_WithBlankEventNumber_ThrowsArgumentException(
        string? eventNumber)
    {
        // ARRANGE
        var creatorId = Guid.NewGuid();
        var createdAt = new DateTimeOffset(
            2026, 10, 15, 5, 0, 0, TimeSpan.Zero);

        // ACT AND ASSERT
        Assert.Throws<ArgumentException>(() =>
            Event.CreateDraft(
                creatorId,
                eventNumber!,
                createdAt));
    }

    [Fact]
    public void UpdateDraft_WithIncompleteInformation_UpdatesAndNormalizesFields()
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 15, 16, 0, 0, TimeSpan.Zero);

        var eventRecord = Event.CreateDraft(
            Guid.NewGuid(),
            "EVT-1047",
            createdAt);

        var occurredAt = new DateTimeOffset(
            2026, 10, 15, 8, 30, 0, TimeSpan.FromHours(-7));

        // ACT
        eventRecord.UpdateDraft(
            title: "  Equipment inspection discrepancy  ",
            description: "   ",
            occurredAt: occurredAt,
            sourceType: "  FacilityReport  ",
            category: "",
            location: null);

        // ASSERT
        Assert.Equal(
            "Equipment inspection discrepancy",
            eventRecord.Title);

        Assert.Null(eventRecord.Description);
        Assert.Equal("FacilityReport", eventRecord.SourceType);
        Assert.Null(eventRecord.Category);
        Assert.Null(eventRecord.Location);

        Assert.Equal(
            occurredAt.ToUniversalTime(),
            eventRecord.OccurredAt);

        Assert.Equal(
            TimeSpan.Zero,
            eventRecord.OccurredAt!.Value.Offset);

        Assert.Equal(EventStatus.Draft, eventRecord.Status);
        Assert.Null(eventRecord.SubmittedAt);
    }


    [Theory]
    [InlineData(null, "An inspection found a discrepancy.", true)]
    [InlineData("Equipment discrepancy", "   ", true)]
    [InlineData("Equipment discrepancy", "An inspection found a discrepancy.", false)]
    public void Submit_WithMissingRequiredInformation_LeavesEventAsDraft(
        string? title,
        string? description,
        bool hasOccurredAt)
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 15, 16, 0, 0, TimeSpan.Zero);

        var eventRecord = Event.CreateDraft(
            Guid.NewGuid(),
            "EVT-1048",
            createdAt);

        DateTimeOffset? occurredAt = hasOccurredAt
            ? createdAt.AddHours(-1)
            : null;

        eventRecord.UpdateDraft(
            title: title,
            description: description,
            occurredAt: occurredAt,
            sourceType: null,
            category: null,
            location: null);

        var submittedAt = createdAt.AddMinutes(10);

        // ACT AND ASSERT
        Assert.Throws<InvalidOperationException>(() =>
            eventRecord.Submit(submittedAt));

        Assert.Equal(EventStatus.Draft, eventRecord.Status);
        Assert.Null(eventRecord.SubmittedAt);
    }

    [Fact]
    public void Submit_WithRequiredInformation_SetsSubmittedStatusAndTime()
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 15, 16, 0, 0, TimeSpan.Zero);

        var eventRecord = Event.CreateDraft(
            Guid.NewGuid(),
            "EVT-1049",
            createdAt);

        var occurredAt = createdAt.AddHours(-1);

        eventRecord.UpdateDraft(
            title: "Equipment inspection discrepancy",
            description: "The recorded setting differs from the installed setting.",
            occurredAt: occurredAt,
            sourceType: null,
            category: null,
            location: null);

        var submittedAt = new DateTimeOffset(
            2026, 10, 15, 9, 15, 0, TimeSpan.FromHours(-7));

        // ACT
        eventRecord.Submit(submittedAt);

        // ASSERT
        Assert.Equal(EventStatus.Submitted, eventRecord.Status);

        Assert.Equal(
            submittedAt.ToUniversalTime(),
            eventRecord.SubmittedAt);

        Assert.Equal(
            TimeSpan.Zero,
            eventRecord.SubmittedAt!.Value.Offset);

        Assert.Equal(createdAt, eventRecord.CreatedAt);
        Assert.Equal(occurredAt, eventRecord.OccurredAt);
    }

    [Fact]
    public void UpdateDraft_AfterSubmission_RejectsChangesAndPreservesOriginalFields()
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 15, 16, 0, 0, TimeSpan.Zero);

        var eventRecord = Event.CreateDraft(
            Guid.NewGuid(),
            "EVT-1050",
            createdAt);

        var occurredAt = createdAt.AddHours(-1);
        var submittedAt = createdAt.AddMinutes(10);

        eventRecord.UpdateDraft(
            title: "Original title",
            description: "Original description",
            occurredAt: occurredAt,
            sourceType: "FacilityReport",
            category: "Equipment",
            location: "Riverside facility");

        eventRecord.Submit(submittedAt);

        // ACT AND ASSERT
        Assert.Throws<InvalidOperationException>(() =>
            eventRecord.UpdateDraft(
                title: "Replacement title",
                description: "Replacement description",
                occurredAt: occurredAt.AddDays(1),
                sourceType: "DifferentSource",
                category: "Different category",
                location: "Different location"));

        Assert.Equal("Original title", eventRecord.Title);
        Assert.Equal("Original description", eventRecord.Description);
        Assert.Equal(occurredAt, eventRecord.OccurredAt);
        Assert.Equal("FacilityReport", eventRecord.SourceType);
        Assert.Equal("Equipment", eventRecord.Category);
        Assert.Equal("Riverside facility", eventRecord.Location);
        Assert.Equal(EventStatus.Submitted, eventRecord.Status);
        Assert.Equal(submittedAt, eventRecord.SubmittedAt);
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_PreservesOriginalSubmissionTime()
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 15, 16, 0, 0, TimeSpan.Zero); // TimeSpan.Zero means UTC offset: zero hours. 

        var eventRecord = Event.CreateDraft(
            Guid.NewGuid(),
            "EVT-1051",
            createdAt);

        eventRecord.UpdateDraft(
            title: "Equipment inspection discrepancy",
            description: "An inspection found a discrepancy.",
            occurredAt: createdAt.AddHours(-1),
            sourceType: null,
            category: null,
            location: null);

        var originalSubmittedAt = createdAt.AddMinutes(10);

        eventRecord.Submit(originalSubmittedAt);

        var secondSubmittedAt = originalSubmittedAt.AddHours(1);

        // ACT AND ASSERT
        Assert.Throws<InvalidOperationException>(() =>
            eventRecord.Submit(secondSubmittedAt));

        Assert.Equal(EventStatus.Submitted, eventRecord.Status);
        Assert.Equal(originalSubmittedAt, eventRecord.SubmittedAt);
    }
}