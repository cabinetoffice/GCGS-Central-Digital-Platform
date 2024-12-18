namespace CO.CDP.MQ;

public interface IEventListenerBackgroundService
{
    Task StartListeningAsync(CancellationToken cancellationToken);
}