using CO.CDP.MQ.Outbox;
using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit.Abstractions;

namespace CO.CDP.MQ.Tests.Outbox;

public class LearnPostgresListenNotifyTest(PostgreSqlFixture postgreSql, ITestOutputHelper testOutputHelper)
    : IClassFixture<PostgreSqlFixture>
{
    private readonly TestDbContext _dbContext = postgreSql.TestDbContext();

    [Fact]
    public async Task ItSendsOutboxNotification()
    {
        var repository = CreateDatabaseOutboxMessageRepository(_dbContext);
        var notifications = new List<string>();

        await using var connection = new NpgsqlConnection(_dbContext.Database.GetConnectionString());
        await connection.OpenAsync();
        connection.Notification += (o, e) =>
        {
            testOutputHelper.WriteLine($"Notification for {e.Channel}: {e.Payload}");
            notifications.Add(e.Channel);
            testOutputHelper.WriteLine($"Notifications for {e.Channel}: {notifications.Count}");
        };

        async Task Listener(NpgsqlConnection npgsqlConnection)
        {
            testOutputHelper.WriteLine($"Listening to notifications");
            await using var listenCommand = new NpgsqlCommand("LISTEN outbox;", npgsqlConnection);
            await listenCommand.ExecuteNonQueryAsync();
        }

        var listener = Listener(connection);

        await repository.SaveAsync(new OutboxMessage
        {
            Message = "Hello World",
            Type = "String"
        });

        await connection.WaitAsync(100);
        await listener;

        notifications.Count.Should().Be(1);
        notifications.Should().BeEquivalentTo(["outbox"]);
    }


    private DatabaseOutboxMessageRepository<TestDbContext> CreateDatabaseOutboxMessageRepository(
        TestDbContext dbContext)
    {
        _dbContext.Database.ExecuteSqlRaw("""
                                          CREATE OR REPLACE FUNCTION outbox_messages_notify()
                                          RETURNS TRIGGER AS $trigger$
                                          DECLARE
                                            payload TEXT;
                                            channel_name TEXT;
                                          BEGIN
                                            IF TG_ARGV[0] IS NULL THEN
                                              RAISE EXCEPTION 'A channel name is required as the first argument';
                                            END IF;

                                            channel_name := TG_ARGV[0];
                                            payload := json_build_object('timestamp', CURRENT_TIMESTAMP, 'payload', row_to_json(NEW));

                                            PERFORM pg_notify(channel_name, payload);

                                            RETURN NEW;
                                          END;
                                          $trigger$ LANGUAGE plpgsql;

                                          CREATE OR REPLACE TRIGGER outbox_messages_notify_trigger
                                          AFTER INSERT ON "OutboxMessages"
                                          FOR EACH ROW EXECUTE PROCEDURE outbox_messages_notify('outbox');
                                          """);
        return new DatabaseOutboxMessageRepository<TestDbContext>(dbContext);
    }
}