using System.Collections.Concurrent;

namespace CO.CDP.MQ.Tests;

public class TestSubscriber<TEvent>(Func<TEvent, Task> handler)
    : ISubscriber<TEvent> where TEvent : class
{
    public readonly ConcurrentBag<object> HandledMessages = new();
    public readonly ConcurrentBag<object> ReceivedMessages = new();

    public TestSubscriber(CancellationTokenSource? cancellationTokenSource = null) : this(
        _ =>
        {
            cancellationTokenSource?.Cancel();
            return Task.CompletedTask;
        })
    {
    }

    public Task Handle(TEvent @event)
    {
        ReceivedMessages.Add(@event);
        var result = handler(@event);
        HandledMessages.Add(@event);
        return result;
    }
}