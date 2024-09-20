using CO.CDP.MQ.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CO.CDP.MQ.Hosting;

public class OutboxProcessorBackgroundService(
    IServiceProvider services,
    OutboxProcessorBackgroundService.OutboxProcessorConfiguration configuration,
    ILogger<OutboxProcessorBackgroundService> logger
) : IHostedService, IDisposable
{
    public record OutboxProcessorConfiguration
    {
        public int BatchSize { get; init; } = 10;
        public TimeSpan ExecutionInterval { get; init; } = TimeSpan.FromSeconds(60);
    }

    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Staring the outbox processor background service Interval={INTERVAL} Batch={BATCH}",
            configuration.ExecutionInterval, configuration.BatchSize);
        _timer = new Timer(ExecuteOutboxProcessorAsync, null, TimeSpan.Zero, configuration.ExecutionInterval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Stopping the outbox processor background service");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private async void ExecuteOutboxProcessorAsync(object? state)
    {
        using var scope = services.CreateScope();
        var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
        await outboxProcessor.ExecuteAsync(configuration.BatchSize);
    }
}