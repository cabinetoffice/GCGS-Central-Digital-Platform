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
        var messagesPublishedBySubscriber1 = new ConcurrentBag<object>();
        var messagesPublishedBySubscriber2 = new ConcurrentBag<object>();

        var dispatcher = await CreateDispatcher();
        dispatcher.Subscribe<TestMessage>(message =>
        {
            messagesPublishedBySubscriber1.Add(message);
            return Task.CompletedTask;
        });
        dispatcher.Subscribe<TestMessage>(message =>
        {
            messagesPublishedBySubscriber2.Add(message);
            tokenSource.Cancel();
            return Task.CompletedTask;
        });

        var task = dispatcher.ExecuteAsync(tokenSource.Token);

        await PublishMessage(new UnexpectedTestMessage(42, "Bye."));
        await PublishMessage(new TestMessage(13, "Hello."));

        await task;

        messagesPublishedBySubscriber1.Should().Equal([new TestMessage(13, "Hello.")]);
        messagesPublishedBySubscriber2.Should().Equal([new TestMessage(13, "Hello.")]);
    }

    [Fact]
    public async Task ItRemovesHandledMessages()
    {
        var tokenSource = new CancellationTokenSource();
        var messagesPublishedBySubscriber = new ConcurrentBag<object>();

        var dispatcher = await CreateDispatcher();
        dispatcher.Subscribe<TestMessage>(message =>
        {
            messagesPublishedBySubscriber.Add(message);
            tokenSource.Cancel();
            return Task.CompletedTask;
        });

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
}