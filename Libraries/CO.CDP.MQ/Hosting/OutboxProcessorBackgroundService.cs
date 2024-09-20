using CO.CDP.MQ.Outbox;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.MQ.Hosting;

public class OutboxProcessorBackgroundService(
    IServiceProvider services,
    OutboxProcessorBackgroundService.OutboxProcessorConfiguration configuration
) : IHostedService, IDisposable
{
    public record OutboxProcessorConfiguration
    {
        public int BatchSize { get; init; } = 10;
        public TimeSpan ExecutionInterval { get; init; } = TimeSpan.FromSeconds(60);
    }

    private Timer? _timer = null;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(ExecuteOutboxProcessorAsync, null, TimeSpan.Zero, configuration.ExecutionInterval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
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