using System.Text.Json;
using CO.CDP.MQ.Database;
using CO.CDP.Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CO.CDP.MQ.Tests.Database;

public class DatabasePublisherContractTest(PostgreSqlFixture postgreSql)
    : PublisherContractTest, IClassFixture<PostgreSqlFixture>
{
    private readonly DatabaseOutboxMessageRepository<TestDbContext> _outboxMessages = new(postgreSql.TestDbContext());

    protected override async Task<T> waitForOneMessage<T>()
    {
        var messages = await _outboxMessages.FindOldest(count: 1);
        var message = messages.First();
        return Deserialize<T>(message) ??
               throw new Exception($"Failed to deserialize the message: {message}.");
    }

    protected override Task<IPublisher> CreatePublisher()
    {
        return Task.FromResult<IPublisher>(new DatabasePublisher(
            _outboxMessages,
            LoggerFactory.Create(_ => { }).CreateLogger<DatabasePublisher>()
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