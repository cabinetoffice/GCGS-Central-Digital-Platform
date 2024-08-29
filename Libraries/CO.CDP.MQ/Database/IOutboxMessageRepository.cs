namespace CO.CDP.MQ.Database;

public interface IOutboxMessageRepository
{
    Task SaveAsync(OutboxMessage message);

    Task<List<OutboxMessage>> FindOldest(int count = 10);
}