using CO.CDP.MQ.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CO.CDP.MQ.Hosting;

public class OutboxProcessorBackgroundService(
    IServiceProvider services,
    OutboxProcessorBackgroundService.OutboxProcessorConfiguration configuration,
    ILogger<OutboxProcessorBackgroundService> logger
) : BackgroundService
{
    public record OutboxProcessorConfiguration
    {
        public int BatchSize { get; init; } = 10;
        public TimeSpan ExecutionInterval { get; init; } = TimeSpan.FromSeconds(60);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug(
            "Staring the outbox processor background service Interval={INTERVAL} Batch={BATCH}",
            configuration.ExecutionInterval, configuration.BatchSize);

        await ExecuteOutboxProcessorAsync();

        using PeriodicTimer timer = new(configuration.ExecutionInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExecuteOutboxProcessorAsync();
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Stopping the outbox processor background service");
        }

    }

    private async Task ExecuteOutboxProcessorAsync()
    {
        using var scope = services.CreateScope();
        var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
        await outboxProcessor.ExecuteAsync(configuration.BatchSize);
    }
}