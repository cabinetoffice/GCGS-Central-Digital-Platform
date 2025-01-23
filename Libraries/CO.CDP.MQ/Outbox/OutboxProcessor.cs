using Microsoft.Extensions.Logging;

namespace CO.CDP.MQ.Outbox;

public interface IOutboxProcessor
{
    Task<int> ExecuteAsync(int count);
}

public class OutboxProcessor(
    IPublisher publisher,
    IOutboxMessageRepository outbox,
    TimeSpan lockTimeout,
    ILogger<OutboxProcessor> logger)
    : IOutboxProcessor
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public OutboxProcessor(IPublisher publisher, IOutboxMessageRepository outbox, ILogger<OutboxProcessor> logger) :
        this(publisher, outbox, TimeSpan.FromSeconds(1), logger)
    {
    }

    public async Task<int> ExecuteAsync(int count)
    {
        logger.LogDebug("Outbox processor waiting for the lock...");
        if (!await _semaphore.WaitAsync(lockTimeout))
        {
            logger.LogDebug("Outbox processor failed to acquire the lock");
            return 0;
        }

        try
        {
            logger.LogDebug("Executing the outbox processor");
            return await PublishMessages(count);
        }
        finally
        {
            logger.LogDebug("Stopping the outbox processor");
            _semaphore.Release();
        }
    }

    private async Task<int> PublishMessages(int count)
    {
        var messages = await FetchMessages(count);
        foreach (var outboxMessage in messages)
        {
            await PublishMessage(outboxMessage);
        }

        return messages.Count;
    }

    private async Task<List<OutboxMessage>> FetchMessages(int count)
    {
        var messages = await outbox.FindOldest(count);
        logger.LogDebug("Fetched {COUNT} message(s)", messages.Count);
        return messages;
    }

    private async Task PublishMessage(OutboxMessage m)
    {
        logger.LogDebug("Publishing the `{TYPE}` message: `{MESSAGE}`", m.Type, m.Message);
        await publisher.Publish(m);

        logger.LogDebug("Marking the message as published");
        m.Published = true;
        await outbox.SaveAsync(m);
    }
}