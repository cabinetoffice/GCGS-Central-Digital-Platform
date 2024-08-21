namespace CO.CDP.MQ.Database;

public interface IOutboxMessageRepository
{
    Task SaveAsync(OutboxMessage message);
}