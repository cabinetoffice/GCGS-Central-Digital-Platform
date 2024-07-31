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

        subscriber1.HandledMessages.Should().Equal([new TestMessage(13, "Hello.")]);
        subscriber2.HandledMessages.Should().Equal([new TestMessage(13, "Hello.")]);
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

    [Fact]
    public async Task ItAttemptsToRecoverFromFailures()
    {
        var tokenSource = new CancellationTokenSource();
        var failingSubscriberCallCount = 0;

        var subscriber = new TestSubscriber<TestMessage>(_ => Task.CompletedTask);
        var failingSubscriber = new TestSubscriber<TestMessage>(_ =>
        {
            failingSubscriberCallCount++;
            if (failingSubscriberCallCount == 1)
            {
                throw new Exception("Failure in subscriber.");
            }
            tokenSource.Cancel();
            return Task.CompletedTask;
        });

        var dispatcher = await CreateDispatcher();
        dispatcher.Subscribe(subscriber);
        dispatcher.Subscribe(failingSubscriber);

        var task = dispatcher.ExecuteAsync(tokenSource.Token);

        await PublishMessage(new TestMessage(13, "Hello."));

        await task;

        subscriber.ReceivedMessages.Should()
            .Equal([new TestMessage(13, "Hello."), new TestMessage(13, "Hello.")]);
        failingSubscriber.ReceivedMessages.Should()
            .Equal([new TestMessage(13, "Hello."), new TestMessage(13, "Hello.")]);
        subscriber.HandledMessages.Should()
            .Equal([new TestMessage(13, "Hello."), new TestMessage(13, "Hello.")]);
        failingSubscriber.HandledMessages.Should()
            .Equal([new TestMessage(13, "Hello.")]);
    }

    protected abstract Task<IDispatcher> CreateDispatcher();
    protected abstract Task PublishMessage<TM>(TM message) where TM : class;
    protected abstract Task<int> CountMessagesInQueue();

    protected record TestMessage(int Id, String Name);

    private record UnexpectedTestMessage(int Id, String Name);

    private class TestSubscriber<TEvent>(Func<TEvent, Task> handler)
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
}