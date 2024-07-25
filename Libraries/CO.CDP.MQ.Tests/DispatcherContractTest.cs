using System.Collections.Concurrent;
using FluentAssertions;

namespace CO.CDP.MQ.Tests;

public abstract class DispatcherContractTest
{
    protected const string TestQueue = "test-queue-2";

    [Fact]
    public async Task ItInvokesSubscribersForMatchingMessageType()
    {
        var tokenSource = new CancellationTokenSource();
        var subscriber1 = new TestSubscriber<TestMessage>();
        var subscriber2 = new TestSubscriber<TestMessage>(tokenSource);

        var dispatcher = await CreateDispatcher();
        dispatcher.Subscribe(subscriber1);
        dispatcher.Subscribe(subscriber2);

        var task = dispatcher.ExecuteAsync(tokenSource.Token);

        await PublishMessage(new UnexpectedTestMessage(42, "Bye."));
        await PublishMessage(new TestMessage(13, "Hello."));

        await task;

        subscriber1.PublishedMessages.Should().Equal([new TestMessage(13, "Hello.")]);
        subscriber2.PublishedMessages.Should().Equal([new TestMessage(13, "Hello.")]);
    }

    [Fact]
    public async Task ItRemovesHandledMessages()
    {
        var tokenSource = new CancellationTokenSource();
        var subscriber = new TestSubscriber<TestMessage>(tokenSource);

        var dispatcher = await CreateDispatcher();
        dispatcher.Subscribe(subscriber);

        var task = dispatcher.ExecuteAsync(tokenSource.Token);

        await PublishMessage(new TestMessage(13, "Hello."));

        await task;

        (await CountMessagesInQueue()).Should().Be(0);
    }

    protected abstract Task<IDispatcher> CreateDispatcher();
    protected abstract Task PublishMessage<TM>(TM message) where TM : class;
    protected abstract Task<int> CountMessagesInQueue();

    protected record TestMessage(int Id, String Name);

    private record UnexpectedTestMessage(int Id, String Name);

    private class TestSubscriber<TEvent>(CancellationTokenSource? tokenSource = null) : ISubscriber<TEvent> where TEvent : class
    {
        public readonly ConcurrentBag<object> PublishedMessages = new();
        public Task Handle(TEvent @event)
        {
            PublishedMessages.Add(@event);
            tokenSource?.Cancel();
            return Task.CompletedTask;
        }
    }
}