using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.MQ.Hosting;

public class DispatcherBackgroundService(IServiceProvider services) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = services.CreateScope();
        using var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();
        await dispatcher.ExecuteAsync(stoppingToken);
    }
}