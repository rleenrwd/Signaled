using Signaled.Application.Events;
using Signaled.Application.Tests.TestDoubles;
using Signaled.Domain.Events;
using Xunit;

namespace Signaled.Application.Tests.Events;

public class CreateEventDraftServiceTests
{
    [Fact]
    public async Task CreateAsync_WithEmptyRequest_CreatesAndSavesDraft()
    {
        // ARRANGE
        var creatorId = Guid.NewGuid();

        var expectedTime = new DateTimeOffset(
            2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

        var generator = new StubEventNumberGenerator();
        var store = new SpyEventDraftStore();
        var clock = new FixedTimeProvider(expectedTime);

        var service = new CreateEventDraftService(
            generator,
            store,
            clock);

        var request = new CreateEventDraftRequest();

        // ACT
        var eventRecord = await service.CreateAsync(
            request,
            creatorId);

        // ASSERT
        Assert.Equal("EVT-0001", eventRecord.EventNumber);
        Assert.Equal(creatorId, eventRecord.CreatedByUserId);
        Assert.Equal(expectedTime, eventRecord.CreatedAt);
        Assert.Equal(EventStatus.Draft, eventRecord.Status);

        Assert.Null(eventRecord.Title);
        Assert.Null(eventRecord.Description);
        Assert.Null(eventRecord.OccurredAt);
        Assert.Null(eventRecord.SubmittedAt);

        Assert.Equal(1, generator.CallCount);
        Assert.Equal(1, store.CallCount);
        Assert.Same(eventRecord, store.SavedEvent);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyCreator_RejectsBeforeGeneratingOrSaving()
    {
        // ARRANGE
        var generator = new StubEventNumberGenerator();
        var store = new SpyEventDraftStore();

        var clock = new FixedTimeProvider(
            new DateTimeOffset(
                2026, 10, 24, 12, 0, 0, TimeSpan.Zero));

        var service = new CreateEventDraftService(
            generator,
            store,
            clock);

        var request = new CreateEventDraftRequest();

        // ACT AND ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(request, Guid.Empty));

        Assert.Equal(0, generator.CallCount);
        Assert.Equal(0, store.CallCount);
        Assert.Null(store.SavedEvent);
    }

    [Fact]
    public async Task CreateAsync_WithInformation_PassesPopulatedDraftToStore()
    {
        // ARRANGE
        var creatorId = Guid.NewGuid();

        var expectedTime = new DateTimeOffset(
            2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

        var generator = new StubEventNumberGenerator();
        var store = new SpyEventDraftStore();
        var clock = new FixedTimeProvider(expectedTime);

        var service = new CreateEventDraftService(
            generator,
            store,
            clock);

        var request = new CreateEventDraftRequest
        {
            Title = "Equipment discrepancy",
            Description = "The installed setting differs from the record.",
            OccurredAt = expectedTime.AddHours(-2),
            SourceType = "FacilityReport",
            Category = "Equipment",
            Location = "Riverside facility"
        };

        // ACT
        var eventRecord = await service.CreateAsync(request, creatorId);

        // ASSERT
        Assert.Equal(request.Title, eventRecord.Title);
        Assert.Equal(request.Description, eventRecord.Description);
        Assert.Equal(request.OccurredAt, eventRecord.OccurredAt);
        Assert.Equal(request.SourceType, eventRecord.SourceType);
        Assert.Equal(request.Category, eventRecord.Category);
        Assert.Equal(request.Location, eventRecord.Location);

        Assert.Equal(EventStatus.Draft, eventRecord.Status);
        Assert.Null(eventRecord.SubmittedAt);
        Assert.Equal(1, store.CallCount);
        Assert.Same(eventRecord, store.SavedEvent);
    }



    [Fact]
    public async Task CreateAsync_WithNullRequest_RejectsBeforeGeneratingOrSaving()
    {
        // ARRANGE
        var generator = new StubEventNumberGenerator();
        var store = new SpyEventDraftStore();

        var clock = new FixedTimeProvider(
            new DateTimeOffset(
                2026, 10, 24, 12, 0, 0, TimeSpan.Zero));

        var service = new CreateEventDraftService(
            generator,
            store,
            clock);

        var creatorId = Guid.NewGuid();

        // ACT AND ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateAsync(null!, creatorId));

        Assert.Equal(0, generator.CallCount);
        Assert.Equal(0, store.CallCount);
        Assert.Null(store.SavedEvent);
    }
}