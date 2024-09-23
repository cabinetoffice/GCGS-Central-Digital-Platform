using CO.CDP.MQ.Outbox;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.MQ.Tests.Outbox;

public class OutboxProcessorTest
{
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<IOutboxMessageRepository> _outbox = new();
    private readonly ILogger<OutboxProcessor> _logger = LoggerFactory.Create(_ => { }).CreateLogger<OutboxProcessor>();

    [Fact]
    public async Task ItPublishesOutboxMessages()
    {
        var messages = new List<OutboxMessage>
        {
            GivenOutboxMessage(type: "Greeting", message: "{\"Message\":\"Hello World\"}", published: false),
            GivenOutboxMessage(type: "Greeting", message: "{\"Message\":\"Good bye\"}", published: false)
        };

        _outbox.Setup(m => m.FindOldest(10)).ReturnsAsync(messages);

        var processor = new OutboxProcessor(_publisher.Object, _outbox.Object, _logger);

        await processor.ExecuteAsync(count: 10);

        _publisher.Verify(p => p.Publish(messages.ElementAt(0)), Times.Once);
        _publisher.Verify(p => p.Publish(messages.ElementAt(1)), Times.Once);
    }

    [Fact]
    public async Task ItMarksPublishedMessagesAsPublished()
    {
        var messages = new List<OutboxMessage>
        {
            GivenOutboxMessage(id: 1, message: "{\"Message\":\"Hello World\"}", published: false),
            GivenOutboxMessage(id: 2, message: "{\"Message\":\"Good bye\"}", published: false)
        };

        _outbox.Setup(m => m.FindOldest(10)).ReturnsAsync(messages);

        var processor = new OutboxProcessor(_publisher.Object, _outbox.Object, _logger);

        await processor.ExecuteAsync(count: 10);

        _outbox.Verify(o => o.SaveAsync(It.Is<OutboxMessage>(
            m => m.Id == 1 && m.Published == true)), Times.Once);
        _outbox.Verify(o => o.SaveAsync(It.Is<OutboxMessage>(
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