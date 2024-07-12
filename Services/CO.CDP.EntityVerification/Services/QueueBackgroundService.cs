using CO.CDP.EntityVerification.SQS;

namespace CO.CDP.EntityVerification.Services;

public class QueueBackgroundService : BackgroundService
{
    private readonly IRequestListener _listener;
    private readonly IQueueProcessor _messageProcessor;

    public QueueBackgroundService(IRequestListener listener,
        IQueueProcessor messageProcessor)
    {
        _listener = listener;
        _messageProcessor = messageProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        _listener.Register(_messageProcessor);
        _messageProcessor.Start(stoppingToken);
    }
}