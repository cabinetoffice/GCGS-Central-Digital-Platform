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
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return ExecuteOutboxProcessorAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private async Task ExecuteOutboxProcessorAsync()
    {
        using var scope = services.CreateScope();
        var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
        await outboxProcessor.ExecuteAsync(configuration.BatchSize);
    }
}