using CO.CDP.MQ.Outbox;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.MQ.Tests.Outbox;

public class OutboxProcessorTest
{
    private readonly Mock<IPublisher> _publisher = new();
    private readonly InMemoryOutboxMessageRepository _outbox = new();
    private readonly ILogger<OutboxProcessor> _logger = LoggerFactory.Create(_ => { }).CreateLogger<OutboxProcessor>();

    [Fact]
    public async Task ItPublishesOutboxMessages()
    {
        var messages = new List<OutboxMessage>
        {
            GivenOutboxMessage(type: "Greeting", message: "{\"Message\":\"Hello World\"}", published: false),
            GivenOutboxMessage(type: "Greeting", message: "{\"Message\":\"Good bye\"}", published: false)
        };
        await GivenMessagesInOutbox(messages);

        var processor = new OutboxProcessor(_publisher.Object, _outbox, _logger);

        await processor.ExecuteAsync(count: 10);

        _publisher.Verify(p => p.Publish(messages.ElementAt(0)), Times.Once);
        _publisher.Verify(p => p.Publish(messages.ElementAt(1)), Times.Once);
    }

    [Fact]
    public async Task ItReturnsTheNumberOfProcessedMessages()
    {
        await GivenMessagesInOutbox([
            GivenOutboxMessage(type: "Greeting", message: "{\"Message\":\"Hello World\"}", published: false),
            GivenOutboxMessage(type: "Greeting", message: "{\"Message\":\"How do you do?\"}", published: false),
            GivenOutboxMessage(type: "Greeting", message: "{\"Message\":\"Good bye\"}", published: false)
        ]);

        var processor = new OutboxProcessor(_publisher.Object, _outbox, _logger);

        var result = await processor.ExecuteAsync(count: 10);

        result.Should().Be(3);
    }

    [Fact]
    public async Task ItMarksPublishedMessagesAsPublished()
    {
        await GivenMessagesInOutbox([
            GivenOutboxMessage(id: 1, message: "{\"Message\":\"Hello World\"}", published: false),
            GivenOutboxMessage(id: 2, message: "{\"Message\":\"Good bye\"}", published: false)
        ]);

        var processor = new OutboxProcessor(_publisher.Object, _outbox, _logger);

        await processor.ExecuteAsync(count: 10);

        _outbox.Messages.Should().Contain(m => m.Id == 1 && m.Published == true);
        _outbox.Messages.Should().Contain(m => m.Id == 2 && m.Published == true);
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

    private async Task GivenMessagesInOutbox(List<OutboxMessage> messages)
    {
        foreach (var message in messages)
        {
            await _outbox.SaveAsync(message);
        }
    }
}

internal class InMemoryOutboxMessageRepository : IOutboxMessageRepository
{
    public readonly List<OutboxMessage> Messages = new();

    public Task SaveAsync(OutboxMessage message)
    {
        Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task<List<OutboxMessage>> FindOldest(int count = 10)
    {
        return Task.FromResult(Messages.FindAll(m => m.Published == false).Take(count).ToList());
    }
}