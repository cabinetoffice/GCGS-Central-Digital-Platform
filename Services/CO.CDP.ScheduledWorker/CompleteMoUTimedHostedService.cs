namespace CO.CDP.ScheduledWorker;

public class CompleteMoUTimedHostedService(
    IServiceProvider services,
    IConfiguration configuration,
    ILogger<CompleteMoUTimedHostedService> logger) : IHostedService, IDisposable
{
    private Timer? _timer = null;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationTokenSource _stoppingCts = new();

    private int JobIntervalInSeconds =>
        configuration.GetValue<int?>("CompleteMoUReminderJob:IntervalInSeconds") ??
                throw new Exception("Missing configuration keys: CompleteMoUReminderJob:IntervalInSeconds.");

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Complete MoU Timed Hosted Service running.");

        stoppingToken.Register(_stoppingCts.Cancel);

        _timer = new Timer(ExecuteWorkAsync, null, TimeSpan.Zero, TimeSpan.FromSeconds(JobIntervalInSeconds));

        return Task.CompletedTask;
    }

    private async void ExecuteWorkAsync(object? state)
    {
        if (!_semaphore.Wait(0))
        {
            logger.LogInformation("Skipping execution - previous work for 'Complete MoU Reminder Service' still in progress");
            return;
        }

        try
        {
            using var scope = services.CreateScope();
            var workService = scope.ServiceProvider
                .GetRequiredService<IScopedProcessingService>();

            await workService.ExecuteWorkAsync(_stoppingCts.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error occurred executing {nameof(ExecuteWorkAsync)}.");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Complete MoU Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}