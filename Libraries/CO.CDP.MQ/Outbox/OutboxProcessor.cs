namespace CO.CDP.MQ.Outbox;

public interface IOutboxProcessor
{
    Task ExecuteAsync(int count);
}

public class OutboxProcessor(IPublisher publisher, IOutboxMessageRepository outbox) : IOutboxProcessor
{
    public async Task ExecuteAsync(int count)
    {
        var messages = await outbox.FindOldest(count);
        messages.ForEach(PublishMessages);
    }

    private async void PublishMessages(OutboxMessage m)
    {
        await publisher.Publish(m);
        m.Published = true;
        await outbox.SaveAsync(m);
    }
}