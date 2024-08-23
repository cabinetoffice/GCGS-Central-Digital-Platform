using CO.CDP.MQ.Database;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;

namespace CO.CDP.MQ.Tests.Database;

public class DatabaseOutboxMessageRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItPersistsAnOutgoingMessage()
    {
        var message = new OutboxMessage
        {
            Message = "Hello World",
            Type = "String"
        };

        var repository = new DatabaseOutboxMessageRepository();
        await repository.SaveAsync(message);

        var foundMessage = await repository.FindOldest(1);

        foundMessage.Should().BeEquivalentTo([message]);
    }
}

class DatabaseOutboxMessageRepository : IOutboxMessageRepository
{
    private List<OutboxMessage> Messages { get; set; } = new();

    public Task SaveAsync(OutboxMessage message)
    {
        Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task<List<OutboxMessage>> FindOldest(int count = 10)
    {
        return Task.FromResult(Messages.Slice(0, count));
    }
}