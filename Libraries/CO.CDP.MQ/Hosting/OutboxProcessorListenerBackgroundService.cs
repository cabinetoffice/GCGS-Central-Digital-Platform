using CO.CDP.MQ.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static CO.CDP.MQ.Hosting.OutboxProcessorBackgroundService;

namespace CO.CDP.MQ.Hosting;

public class OutboxProcessorListenerBackgroundService(
    IServiceProvider services,
    OutboxProcessorConfiguration configuration,
    ILogger<OutboxProcessorListenerBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug(
            "Staring the outbox processor listener background service Batch={BATCH}",
            configuration.BatchSize);

        try
        {
            await StartOutboxProcessorListenerAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Stopping the outbox processor listener background service");
        }
    }

    private async Task StartOutboxProcessorListenerAsync(CancellationToken stoppingToken)
    {
        using var scope = services.CreateScope();
        var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessorListener>();
        await outboxProcessor.WaitAsync(configuration.BatchSize, stoppingToken);
    }
}