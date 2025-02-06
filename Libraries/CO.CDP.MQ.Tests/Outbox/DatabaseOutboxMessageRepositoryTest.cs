using CO.CDP.MQ.Outbox;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CO.CDP.MQ.Tests.Outbox;

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
            Published = false,
            QueueUrl = "test-queue",
            MessageGroupId = "test-messages"
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
            new OutboxMessage { Message = "Message 0", Type = "String", Published = true, QueueUrl = "test-queue", MessageGroupId = "test-messages" },
            new OutboxMessage { Message = "Message 1", Type = "String", Published = false, QueueUrl = "test-queue", MessageGroupId = "test-messages" },
            new OutboxMessage { Message = "Message 2", Type = "String", Published = false, QueueUrl = "test-queue", MessageGroupId = "test-messages" },
            new OutboxMessage { Message = "Message 3", Type = "String", Published = false, QueueUrl = "test-queue", MessageGroupId = "test-messages" }
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

    [Fact]
    public async Task ItSendsOutboxNotificationOnSave()
    {
        var repository = CreateDatabaseOutboxMessageRepository(_dbContext);

        await using var connection = new NpgsqlConnection(_dbContext.Database.GetConnectionString());
        await connection.OpenAsync();
        var listener = new NotificationListener(connection, "outbox");

        await repository.SaveAsync(new OutboxMessage
        {
            Message = "Hello World",
            Type = "String",
            QueueUrl = "test-queue",
            MessageGroupId = "test-messages"
        });

        await connection.WaitAsync(100);
        await listener.WaitAsync();

        listener.Notifications.Count.Should().Be(1);
        listener.Notifications.Should().BeEquivalentTo(["outbox"]);
    }

    private DatabaseOutboxMessageRepository<TestDbContext> CreateDatabaseOutboxMessageRepository(
        TestDbContext dbContext)
    {
        return new DatabaseOutboxMessageRepository<TestDbContext>(dbContext);
    }
}

internal class NotificationListener
{
    public readonly List<string> Notifications = new();
    private readonly Task _listener;

    public NotificationListener(NpgsqlConnection connection, string channel)
    {
        connection.Notification += (_, e) =>
        {
            Notifications.Add(e.Channel);
        };

        async Task Listener(NpgsqlConnection npgsqlConnection)
        {
            await using var listenCommand = new NpgsqlCommand($"LISTEN {channel};", npgsqlConnection);
            await listenCommand.ExecuteNonQueryAsync();
        }

        _listener = Listener(connection);
    }

    public async Task WaitAsync()
    {
        await _listener;
    }
}