using Amazon.SQS.Model;

namespace CO.CDP.EntityVerification.SQS;

public interface IQueueProcessor
{
    public delegate void EvEventHandler(Message msg);
    public event EvEventHandler OnNewMessage;

    public void Start(CancellationToken stoppingToken);
}