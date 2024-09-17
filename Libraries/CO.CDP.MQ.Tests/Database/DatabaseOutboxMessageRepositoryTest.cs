using CO.CDP.MQ.Database;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;

namespace CO.CDP.MQ.Tests.Database;

public class DatabaseOutboxMessageRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    private readonly TestDbContext _dbContext = postgreSql.TestDbContext();

    [Fact]
    public async Task ItPersistsAnOutgoingMessage() => await _dbContext.InvokeIsolated(async () =>
    {
        var repository = CreateDatabaseOutboxMessageRepository(_dbContext);

        var message = new OutboxMessage
        {
            Message = "Hello World",
            Type = "String",
            Published = false
        };
        await repository.SaveAsync(message);

        var foundMessages = await repository.FindOldest(1);

        foundMessages.Count.Should().Be(1);
        foundMessages[0].Message.Should().Be("Hello World");
        foundMessages[0].Type.Should().Be("String");
        foundMessages[0].Published.Should().BeFalse();
        foundMessages[0].CreatedOn.Should().BeWithin(TimeSpan.FromSeconds(5)).Before(DateTimeOffset.Now);
        foundMessages[0].UpdatedOn.Should().BeWithin(TimeSpan.FromSeconds(5)).Before(DateTimeOffset.Now);
    });

    [Fact]
    public async Task ItFindsTheOldestOutgoingMessages() => await _dbContext.InvokeIsolated(async () =>
    {
        var repository = CreateDatabaseOutboxMessageRepository(_dbContext);

        List<OutboxMessage> messages =
        [
            new OutboxMessage { Message = "Message 0", Type = "String", Published = true },
            new OutboxMessage { Message = "Message 1", Type = "String", Published = false },
            new OutboxMessage { Message = "Message 2", Type = "String", Published = false },
            new OutboxMessage { Message = "Message 3", Type = "String", Published = false }
        ];
        foreach (var message in messages)
        {
            await repository.SaveAsync(message);
        }

        var foundMessages = await repository.FindOldest(2);

        foundMessages.Count.Should().Be(2);
        foundMessages[0].Message.Should().Be("Message 1");
        foundMessages[1].Message.Should().Be("Message 2");
    });

    private DatabaseOutboxMessageRepository<TestDbContext> CreateDatabaseOutboxMessageRepository(
        TestDbContext dbContext)
    {
        return new DatabaseOutboxMessageRepository<TestDbContext>(dbContext);
    }
}