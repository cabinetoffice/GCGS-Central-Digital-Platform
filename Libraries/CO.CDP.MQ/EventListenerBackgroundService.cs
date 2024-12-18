using CO.CDP.MQ.Outbox;
using Npgsql;

namespace CO.CDP.MQ;

public class EventListenerBackgroundService : IEventListenerBackgroundService
{
    private readonly string _connectionString;
    private readonly IOutboxMessageRepository _repository;

    public EventListenerBackgroundService(string connectionString, IOutboxMessageRepository repository)
    {
        _connectionString = connectionString;
        _repository = repository;
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        connection.Notification += (sender, e) =>
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                // Handle notification
            }
        };

        await Listener(connection, cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            await connection
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task Listener(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand($"LISTEN outbox;", connection);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}