using CO.CDP.MQ.Database;
using Moq;

namespace CO.CDP.MQ.Tests.Database;

public class OutboxProcessorTest
{
    [Fact]
    public async Task ItPublishesOutboxMessages()
    {
        var publisher = new Mock<IPublisher>();
        var outbox = new Mock<IOutboxMessageRepository>();

        var messages = new List<OutboxMessage>
        {
            GivenOutboxMessage(type: "Greeting", message: "{\"Message\":\"Hello World\"}", published: false),
            GivenOutboxMessage(type: "Greeting", message: "{\"Message\":\"Good bye\"}", published: false)
        };

        outbox.Setup(m => m.FindOldest(10)).ReturnsAsync(messages);

        var processor = new OutboxProcessor(publisher.Object, outbox.Object);

        await processor.Execute(count: 10);

        publisher.Verify(p => p.Publish(messages.ElementAt(0)), Times.Once);
        publisher.Verify(p => p.Publish(messages.ElementAt(1)), Times.Once);
    }

    private static OutboxMessage GivenOutboxMessage(string type, string message, bool published)
    {
        return new()
        {
            Id = default,
            Type = type,
            Message = message,
            Published = published,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
    }
}

class OutboxProcessor(IPublisher publisher, IOutboxMessageRepository outbox)
{
    public async Task Execute(int count)
    {
        var messages = await outbox.FindOldest(count);
        messages.ForEach(m => publisher.Publish(m));
    }
}