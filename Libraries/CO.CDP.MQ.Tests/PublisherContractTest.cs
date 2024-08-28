using FluentAssertions;

namespace CO.CDP.MQ.Tests;

public abstract class PublisherContractTest
{
    [Fact]
    public async Task ItPublishesMessageToTheQueue()
    {
        var publisher = await CreatePublisher();

        await publisher.Publish(new TestMessage(42, "Hello, Earth!"));

        var message = await waitForOneMessage<TestMessage>();

        message.Should().Be(new TestMessage(42, "Hello, Earth!"));
    }

    protected abstract Task<T> waitForOneMessage<T>() where T : class;

    protected abstract Task<IPublisher> CreatePublisher();

    protected record TestMessage(int Id, String Name);
}