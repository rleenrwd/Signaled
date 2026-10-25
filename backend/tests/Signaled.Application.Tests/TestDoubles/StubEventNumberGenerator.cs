using Signaled.Application.Events;

namespace Signaled.Application.Tests.TestDoubles;

public class StubEventNumberGenerator : IEventNumberGenerator
{
    public int CallCount { get; private set; } 

    public Task<string> GenerateAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CallCount++;

        return Task.FromResult("EVT-0001");
    }
}