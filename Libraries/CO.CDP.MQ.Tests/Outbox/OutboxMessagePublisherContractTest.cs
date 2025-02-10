using System.Text.Json;
using CO.CDP.MQ.Outbox;
using CO.CDP.Testcontainers.PostgreSql;
using Microsoft.Extensions.Logging;

namespace CO.CDP.MQ.Tests.Outbox;

public class OutboxMessagePublisherContractTest(PostgreSqlFixture postgreSql)
    : PublisherContractTest, IClassFixture<PostgreSqlFixture>
{
    private readonly DatabaseOutboxMessageRepository<TestDbContext> _outboxMessages = new(postgreSql.TestDbContext());

    private readonly OutboxMessagePublisher.OutboxMessagePublisherConfiguration _defaultConfiguration = new()
    {
        QueueUrl = "queue://",
        MessageGroupId = "group-a"
    };

    protected override async Task<T> waitForOneMessage<T>()
    {
        var messages = await _outboxMessages.FindOldest(count: 1);
        var message = messages.First();
        return Deserialize<T>(message) ??
               throw new Exception($"Failed to deserialize the message: {message}.");
    }

    protected override Task<IPublisher> CreatePublisher()
    {
        return Task.FromResult<IPublisher>(new OutboxMessagePublisher(
            _outboxMessages,
            _defaultConfiguration,
            LoggerFactory.Create(_ => { }).CreateLogger<OutboxMessagePublisher>()
        ));
    }

    private static T? Deserialize<T>(OutboxMessage message) where T : class
    {
        switch (message.Type)
        {
            case "TestMessage":
                return JsonSerializer.Deserialize<TestMessage>(message.Message) as T;
            default:
                throw new NotImplementedException($"Message type not implemented: `{message.Type}`.");
        }
    }
}