using Microsoft.Extensions.Logging;
using Npgsql;

namespace CO.CDP.MQ.Outbox;

public interface IOutboxProcessorListener
{
    Task WaitAsync(int batchSize, CancellationToken cancellationToken);
}

public class OutboxProcessorListener(
    NpgsqlDataSource dataSource,
    IOutboxProcessor processor,
    ILogger<OutboxProcessorListener> logger,
    string channel = "outbox"
) : IOutboxProcessorListener
{
    public async Task WaitAsync(int batchSize, CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting the outbox processor listener");
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await ProcessOutbox(batchSize, cancellationToken);
        await WaitForMessages(batchSize, connection, cancellationToken);
    }

    private async Task ProcessOutbox(int batchSize, CancellationToken cancellationToken)
    {
        int processedCount = 0;
        do
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug("Delegating to the outbox processor");
                processedCount = await processor.ExecuteAsync(batchSize);
            }
        } while (!cancellationToken.IsCancellationRequested && batchSize <= processedCount);
    }

    private async Task WaitForMessages(int batchSize, NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        connection.Notification += async (_, e) =>
        {
            logger.LogDebug("Received notification for the `{channel}` channel: `{payload}", e.Channel, e.Payload);
            await ProcessOutbox(batchSize, cancellationToken);
        };

        async Task Listener(NpgsqlConnection npgsqlConnection)
        {
            logger.LogDebug("Listening to the `{channel}` channel", channel);
            await using var listenCommand = new NpgsqlCommand($"LISTEN {channel};", npgsqlConnection);
            await listenCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await Listener(connection);

        while (!cancellationToken.IsCancellationRequested)
        {
            await connection.WaitAsync(cancellationToken);
        }
    }
}