using System.Text.Json;
using CO.CDP.MQ.Outbox;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.MQ.Tests.Outbox;

public class OutboxMessagePublisherTest
{
    private readonly Mock<IOutboxMessageRepository> _repository = new();
    private OutboxMessagePublisher Publisher => new(
        _repository.Object,
        o => JsonSerializer.Serialize(o),
        o => o.GetType().Name,
        LoggerFactory.Create(_ => { }).CreateLogger<OutboxMessagePublisher>()
    );

    [Fact]
    public async Task ItPersistsTheMessageInTheDatabase()
    {
        await Publisher.Publish(new TestMessage(13, "Hello, database."));

        _repository.Verify(r => r.SaveAsync(It.IsAny<OutboxMessage>()), Times.Once);

        _repository.Invocations.First().Arguments[0].Should().BeEquivalentTo(
            new OutboxMessage
            {
                Type = "TestMessage",
                Message = """{"Id":13,"Name":"Hello, database."}"""
            }
        );
    }

    private record TestMessage(int Id, String Name);
}