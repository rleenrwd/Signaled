using Microsoft.EntityFrameworkCore;
using Signaled.Domain.Events;
using Xunit;

namespace Signaled.Infrastructure.Tests.Events;

public class EventPersistenceTests : IClassFixture<SqlServerFixture> // xUnit initializes one SqlServerfixture for this test class, shares it among the class's tests, and disposes it after they finish
{
    private readonly SqlServerFixture _fixture;

    public EventPersistenceTests(SqlServerFixture fixture) // Gives this test class a fixture object, allowing our tests to call the fixtures methods.
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateDraft_CanBeSavedAndLoaded()
    {
        // ARRANGE
        var creatorId = Guid.NewGuid();

        var createdAt = new DateTimeOffset(
            2026, 10, 17, 3, 0, 0, TimeSpan.Zero);

        var eventRecord = Event.CreateDraft(
            creatorId,
            "EVT-2001",
            createdAt);

        // ACT: SAVE USING ONE CONTEXT
        await using (var writeContext = _fixture.CreateContext()) // Creates a db context that will be disposed after saving the changes (the disposal is because of await using)
        {
            writeContext.Events.Add(eventRecord); // Tracks the event as Added, ready to be inserted when SaveChangesAsync runs.
            await writeContext.SaveChangesAsync(); // Inserts the event into the database, the "await using" disposes writeContext when this block ends.
        }

        // READ USING A DIFFERENT CONTEXT TO CHECK THAT THE EVENT ACTUALLY COMES BACK FROM SQL SERVER
        await using var readContext = _fixture.CreateContext(); // Creates another db context that will be disposed after its unit of work is complete (when the test method finishes, because it doesn't have a separate braced block of code like write context does)

        var savedEvent = await readContext.Events 
            .SingleAsync(e => e.Id == eventRecord.Id); // Finds and stores the event with the ID we are looking for, making sure exactly one matches.

        // ASSERT
        Assert.Equal("EVT-2001", savedEvent.EventNumber);
        Assert.Equal(creatorId, savedEvent.CreatedByUserId);
        Assert.Equal(createdAt, savedEvent.CreatedAt);
        Assert.Equal(EventStatus.Draft, savedEvent.Status);
        Assert.Null(savedEvent.Title);
        Assert.Null(savedEvent.Description);
        Assert.Null(savedEvent.OccurredAt);
        Assert.Null(savedEvent.SubmittedAt);

        var rowVersion = readContext.Entry(savedEvent)
            .Property<byte[]>("RowVersion")
            .CurrentValue;

        Assert.NotNull(rowVersion);
        Assert.Equal(8, rowVersion.Length); // Makes sure the value in this rowversion column is the expected 8-byte value.
    }


    [Fact]
    public async Task SaveChanges_WithDuplicateEventNumber_ThrowsDbUpdateException()
    {
        // ARRANGE
        var createdAt = new DateTimeOffset(
            2026, 10, 17, 3, 0, 0, TimeSpan.Zero);

        var firstEvent = Event.CreateDraft(
            Guid.NewGuid(),
            "EVT-2002",
            createdAt);

        var secondEvent = Event.CreateDraft(
            Guid.NewGuid(),
            "EVT-2002",
            createdAt);

        await using (var firstContext = _fixture.CreateContext())
        {
            firstContext.Events.Add(firstEvent);
            await firstContext.SaveChangesAsync();
        }

        await using var secondContext = _fixture.CreateContext();

        secondContext.Events.Add(secondEvent);

        // ACT AND ASSERT
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            secondContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WithStaleRowVersion_RejectsUpdate()
    {
        // ARRANGE: SAVE A DRAFT
        var eventRecord = Event.CreateDraft(
            Guid.NewGuid(),
            "EVT-2003",
            new DateTimeOffset(
                2026, 10, 17, 3, 0, 0, TimeSpan.Zero));

        await using (var setupContext = _fixture.CreateContext())
        {
            setupContext.Events.Add(eventRecord);
            await setupContext.SaveChangesAsync();
        }

        // Both contexts load the same version of the event.
        await using var firstContext = _fixture.CreateContext();
        await using var secondContext = _fixture.CreateContext();

        var firstCopy = await firstContext.Events
            .SingleAsync(e => e.Id == eventRecord.Id);

        var secondCopy = await secondContext.Events
            .SingleAsync(e => e.Id == eventRecord.Id);

        // The first update succeeds and changes the database RowVersion.
        firstCopy.UpdateDraft(
            title: "First user's update",
            description: null,
            occurredAt: null,
            sourceType: null,
            category: null,
            location: null);

        await firstContext.SaveChangesAsync();

        // The second context still holds the original RowVersion.
        secondCopy.UpdateDraft(
            title: "Second user's update",
            description: null,
            occurredAt: null,
            sourceType: null,
            category: null,
            location: null);

        // ACT AND ASSERT: THE STALE UPDATE MUST FAIL
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            secondContext.SaveChangesAsync());

        // Verify that the first user's saved change remains intact.
        await using var verificationContext = _fixture.CreateContext();

        var savedEvent = await verificationContext.Events
            .SingleAsync(e => e.Id == eventRecord.Id);

        Assert.Equal("First user's update", savedEvent.Title);
    }



}