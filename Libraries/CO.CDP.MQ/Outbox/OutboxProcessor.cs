using Microsoft.Extensions.Logging;

namespace CO.CDP.MQ.Outbox;

public interface IOutboxProcessor
{
    Task ExecuteAsync(int count);
}

public class OutboxProcessor(IPublisher publisher, IOutboxMessageRepository outbox, ILogger<OutboxProcessor> logger)
    : IOutboxProcessor
{
    public async Task ExecuteAsync(int count)
    {
        logger.LogDebug("Executing the outbox processor");
        var messages = await FetchMessages(count);
        messages.ForEach(PublishMessage);
    }

    private async Task<List<OutboxMessage>> FetchMessages(int count)
    {
        var messages = await outbox.FindOldest(count);
        logger.LogDebug("Fetched {COUNT} message(s)", messages.Count);
        return messages;
    }

    private async void PublishMessage(OutboxMessage m)
    {
        logger.LogDebug("Publishing the `{TYPE}` message: `{MESSAGE}`", m.Type, m.Message);
        await publisher.Publish(m);

        logger.LogDebug("Marking the message as published");
        m.Published = true;
        await outbox.SaveAsync(m);
    }
}