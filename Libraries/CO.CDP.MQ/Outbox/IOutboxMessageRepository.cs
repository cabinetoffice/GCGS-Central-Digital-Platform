namespace CO.CDP.MQ.Outbox;

public interface IOutboxMessageRepository
{
    Task SaveAsync(OutboxMessage message);

    Task<List<OutboxMessage>> FindOldest(int count = 10);
}