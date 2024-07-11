using CO.CDP.EntityVerification.SQS;

namespace CO.CDP.EntityVerification.Services;

public class QueueBackgroundService : BackgroundService
{
    private readonly IRequestListener _listener;
    private readonly IQueueProcessor _processor;

    public QueueBackgroundService(IRequestListener listener, IQueueProcessor messageReceiver)
    {
        _listener = listener;
        _processor = messageReceiver;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        _listener.Register(_processor);
        _processor.Start(stoppingToken);
    }
}