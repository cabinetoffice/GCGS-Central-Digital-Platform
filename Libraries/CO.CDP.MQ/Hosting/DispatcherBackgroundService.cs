using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.MQ.Hosting;

public class DispatcherBackgroundService(IServiceProvider services) : BackgroundService
{
    private readonly TimeSpan _delayBetweenDispatcherExecutions = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ExecuteDispatcherAsync(stoppingToken);
            await Task.Delay(_delayBetweenDispatcherExecutions, stoppingToken);
        }
    }

    private async Task ExecuteDispatcherAsync(CancellationToken stoppingToken)
    {
        using var scope = services.CreateScope();
        using var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();
        await dispatcher.ExecuteAsync(stoppingToken);
    }
}