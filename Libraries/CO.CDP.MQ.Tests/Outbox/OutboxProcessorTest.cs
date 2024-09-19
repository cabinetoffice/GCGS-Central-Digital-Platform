using CO.CDP.MQ.Outbox;
using Moq;

namespace CO.CDP.MQ.Tests.Outbox;

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

        await processor.ExecuteAsync(count: 10);

        publisher.Verify(p => p.Publish(messages.ElementAt(0)), Times.Once);
        publisher.Verify(p => p.Publish(messages.ElementAt(1)), Times.Once);
    }

    [Fact]
    public async Task ItMarksPublishedMessagesAsPublished()
    {
        var publisher = new Mock<IPublisher>();
        var outbox = new Mock<IOutboxMessageRepository>();

        var messages = new List<OutboxMessage>
        {
            GivenOutboxMessage(id: 1, message: "{\"Message\":\"Hello World\"}", published: false),
            GivenOutboxMessage(id: 2, message: "{\"Message\":\"Good bye\"}", published: false)
        };

        outbox.Setup(m => m.FindOldest(10)).ReturnsAsync(messages);

        var processor = new OutboxProcessor(publisher.Object, outbox.Object);

        await processor.ExecuteAsync(count: 10);

        outbox.Verify(o => o.SaveAsync(It.Is<OutboxMessage>(
            m => m.Id == 1 && m.Published == true)), Times.Once);
        outbox.Verify(o => o.SaveAsync(It.Is<OutboxMessage>(
            m => m.Id == 2 && m.Published == true)), Times.Once);
    }

    private static OutboxMessage GivenOutboxMessage(
        int id = default,
        string type = "Greeting",
        string message = "{}",
        bool published = false
    ) => new()
    {
        Id = id,
        Type = type,
        Message = message,
        Published = published,
        CreatedOn = DateTimeOffset.UtcNow,
        UpdatedOn = DateTimeOffset.UtcNow
    };
}