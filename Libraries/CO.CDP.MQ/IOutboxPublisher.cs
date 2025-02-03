using CO.CDP.MQ.Outbox;

namespace CO.CDP.MQ;

public interface IOutboxPublisher : IDisposable
{
    Task Publish(OutboxMessage message);
}