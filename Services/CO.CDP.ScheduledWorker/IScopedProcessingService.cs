namespace CO.CDP.ScheduledWorker;

public interface IScopedProcessingService
{
    Task ExecuteWorkAsync(CancellationToken stoppingToken);
}