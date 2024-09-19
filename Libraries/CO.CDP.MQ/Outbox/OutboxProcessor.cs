namespace CO.CDP.MQ.Outbox;

public class OutboxProcessor(IPublisher publisher, IOutboxMessageRepository outbox)
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