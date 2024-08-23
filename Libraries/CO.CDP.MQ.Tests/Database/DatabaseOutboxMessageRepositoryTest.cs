using CO.CDP.MQ.Database;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;

namespace CO.CDP.MQ.Tests.Database;

public class DatabaseOutboxMessageRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItPersistsAnOutgoingMessage()
    {
        var repository = new DatabaseOutboxMessageRepository();

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
        var repository = new DatabaseOutboxMessageRepository();

        List<OutboxMessage> messages =
        [
            new OutboxMessage
            {
                Message = "Message 2",
                Type = "String",
                CreatedOn = DateTimeOffset.Parse("2021-04-05 11:11:11"),
            },
            new OutboxMessage
            {
                Message = "Message 3",
                Type = "String",
                CreatedOn = DateTimeOffset.Parse("2021-04-05 10:10:10"),
            },
            new OutboxMessage
            {
                Message = "Message 1",
                Type = "String",
                CreatedOn = DateTimeOffset.Parse("2021-04-05 09:09:09"),
            },
        ];
        foreach (var message in messages)
        {
            await repository.SaveAsync(message);
        }

        var foundMessage = await repository.FindOldest(2);

        foundMessage.Should().BeEquivalentTo([messages[2], messages[0]]);
    }
}

class DatabaseOutboxMessageRepository : IOutboxMessageRepository
{
    private List<OutboxMessage> Messages { get; set; } = new();

    public Task SaveAsync(OutboxMessage message)
    {
        Messages.Add(message);
        Messages.Sort((l, r) => l.CreatedOn.CompareTo(l.UpdatedOn));
        return Task.CompletedTask;
    }

    public Task<List<OutboxMessage>> FindOldest(int count = 10)
    {
        return Task.FromResult(Messages.Slice(0, count));
    }
}