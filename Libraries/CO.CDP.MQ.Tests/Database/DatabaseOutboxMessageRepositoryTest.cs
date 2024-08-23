using CO.CDP.MQ.Database;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;

namespace CO.CDP.MQ.Tests.Database;

public class DatabaseOutboxMessageRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItPersistsAnOutgoingMessage()
    {
        var repository = CreateDatabaseOutboxMessageRepository();

        var message = new OutboxMessage
        {
            Message = "Hello World",
            Type = "String"
        };
        await repository.SaveAsync(message);

        var foundMessage = await repository.FindOldest(1);

        foundMessage.Should().BeEquivalentTo([message]);
    }

    [Fact]
    public async Task ItFindsTheOldestOutgoingMessage()
    {
        var repository = CreateDatabaseOutboxMessageRepository();

        List<OutboxMessage> messages =
        [
            new OutboxMessage { Message = "Message 3", CreatedOn = SameDayAt("11:11:11"), Type = "String" },
            new OutboxMessage { Message = "Message 2", CreatedOn = SameDayAt("10:10:10"), Type = "String" },
            new OutboxMessage { Message = "Message 1", CreatedOn = SameDayAt("09:09:09"), Type = "String" }
        ];
        foreach (var message in messages)
        {
            await repository.SaveAsync(message);
        }

        var foundMessages = await repository.FindOldest(2);

        foundMessages.Count.Should().Be(2);
        foundMessages[0].Message.Should().Be("Message 1");
        foundMessages[1].Message.Should().Be("Message 2");
    }

    private static DateTimeOffset SameDayAt(string time)
    {
        return DateTimeOffset.Parse($"2021-04-05T{time}+00:00");
    }

    private DatabaseOutboxMessageRepository<TestDbContext> CreateDatabaseOutboxMessageRepository()
    {
        return new DatabaseOutboxMessageRepository<TestDbContext>(postgreSql.TestDbContext());
    }
}