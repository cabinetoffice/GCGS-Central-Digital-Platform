using CO.CDP.MQ;

namespace CO.CDP.EntityVerification.MQ;

public class QueueBackgroundService(IServiceProvider services) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = services.CreateScope();
        using var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();
        await dispatcher.ExecuteAsync(stoppingToken);
    }
}