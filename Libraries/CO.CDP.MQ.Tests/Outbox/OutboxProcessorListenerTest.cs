using CO.CDP.MQ.Outbox;
using CO.CDP.Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Range = Moq.Range;

namespace CO.CDP.MQ.Tests.Outbox;

public class OutboxProcessorListenerTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    private readonly TestDbContext _dbContext = postgreSql.TestDbContext();
    private readonly CancellationTokenSource _tokenSource = new();
    private readonly Mock<IOutboxProcessor> _processor = new();
    private readonly ILogger<OutboxProcessorListener> _logger =
        LoggerFactory.Create(_ => { }).CreateLogger<OutboxProcessorListener>();
    private int _messagesToBeProcessedCount;

    [Fact]
    public async Task ItInvokesTheOutboxProcessorOnEveryNotification()
    {
        GivenOutboxProcessorPublishesMessagesWithNoInterruptions();

        var connection = await MakeDbConnection();
        var listener = new OutboxProcessorListener(connection, _processor.Object, _logger);

        _ = listener.WaitAsync(10, _tokenSource.Token);

        await GivenOutboxMessagesAreCreated([
            OutboxMessage(message: "Hello!", published: false),
            OutboxMessage(message: "Hello, again!", published: false),
        ]);

        await WaitForMessagesToBeProcessed(100);

        // We pull up to 10 messages at a time and expect 3 calls:
        // * 1 before we start listening to notifications
        // * 2 notifications
        _processor.Verify(p => p.ExecuteAsync(10), Times.Between(2, 3, Range.Inclusive));
    }

    [Fact]
    public async Task ItPullsMessagesUntilThePullIsExhaustedBeforeSwitchingToListeningMode()
    {
        GivenOutboxProcessorPublishesMessagesWithNoInterruptions();

        var connection = await MakeDbConnection();
        var listener = new OutboxProcessorListener(connection, _processor.Object, _logger);

        await GivenOutboxMessagesAreCreated([
            OutboxMessage(message: "Hello!", published: false),
            OutboxMessage(message: "Hello, again!", published: false),
        ]);

        _ = listener.WaitAsync(1, _tokenSource.Token);

        await GivenOutboxMessagesAreCreated([
            OutboxMessage(message: "Bye!", published: false),
            OutboxMessage(message: "Bye, again!", published: false),
        ]);

        await WaitForMessagesToBeProcessed(100);

        // We pull up to 1 message at a time and expect 7 calls:
        // * 3 before we start listening to notifications (3rd pulls 0)
        // * 4 notifications (twice for each message, every other one pulls 0)
        _processor.Verify(p => p.ExecuteAsync(1), Times.Between(6, 7, Range.Inclusive));
    }

    private void GivenOutboxProcessorPublishesMessagesWithNoInterruptions()
    {
        // There's a relationship between the number of messages persisted via the OutboxMessageRepository
        // and the number of messages possible to be processed by the OutboxProcessor.
        // It is capture in the ReturnsAsync code below.
        _processor.Setup(p => p.ExecuteAsync(It.IsAny<int>()))
            .ReturnsAsync((int batchSize) =>
            {
                var processed = batchSize >= _messagesToBeProcessedCount ? _messagesToBeProcessedCount : batchSize;
                _messagesToBeProcessedCount -= processed;
                return processed;
            });
    }

    private async Task WaitForMessagesToBeProcessed(int timeout)
    {
        while (_messagesToBeProcessedCount > 0 && timeout > 0)
        {
            var delay = 1;
            timeout -= delay;
            await Task.Delay(delay);
        }

        await _tokenSource.CancelAsync();
    }

    private static OutboxMessage OutboxMessage(
        string type = "Greeting",
        string message = "Hello.",
        bool published = false
    )
    {
        return new OutboxMessage()
        {
            Type = type,
            Message = message,
            Published = published
        };
    }

    private async Task GivenOutboxMessagesAreCreated(List<OutboxMessage> messages)
    {
        var repository = CreateDatabaseOutboxMessageRepository(_dbContext);
        foreach (var message in messages)
        {
            _messagesToBeProcessedCount += 1;
            await repository.SaveAsync(message);
        }
    }

    private async Task<NpgsqlConnection> MakeDbConnection()
    {
        NpgsqlConnection connection = new NpgsqlConnection(_dbContext.Database.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }

    private DatabaseOutboxMessageRepository<TestDbContext> CreateDatabaseOutboxMessageRepository(
        TestDbContext dbContext)
    {
        return new DatabaseOutboxMessageRepository<TestDbContext>(dbContext);
    }
}