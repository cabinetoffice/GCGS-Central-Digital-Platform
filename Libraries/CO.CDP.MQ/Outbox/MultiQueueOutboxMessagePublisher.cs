using System.Text.Json;
using Microsoft.Extensions.Logging;
using static CO.CDP.MQ.Outbox.OutboxMessagePublisher;

namespace CO.CDP.MQ.Outbox;

public record MultiQueueOutboxMessagePublisherConfiguration
{
    public IReadOnlyList<OutboxMessagePublisherConfiguration> Destinations { get; init; } = [];
}

public class MultiQueueOutboxMessagePublisher(
    IOutboxMessageRepository messages,
    Serializer serializer,
    TypeMapper typeMapper,
    MultiQueueOutboxMessagePublisherConfiguration configuration,
    ILogger<MultiQueueOutboxMessagePublisher> logger
) : IPublisher
{
    public MultiQueueOutboxMessagePublisher(
        IOutboxMessageRepository messages,
        MultiQueueOutboxMessagePublisherConfiguration configuration,
        ILogger<MultiQueueOutboxMessagePublisher> logger)
        : this(messages, o => JsonSerializer.Serialize(o), type => type.GetType().Name, configuration, logger)
    {
    }

    public async Task Publish<TM>(TM message) where TM : notnull
    {
        var messageType = typeMapper(message);
        var serialized = serializer(message);

        logger.LogDebug(
            "Publishing the `{TYPE}` message to {DESTINATIONS} destination queue(s): `{MESSAGE}`",
            messageType,
            configuration.Destinations.Count,
            serialized
        );

        foreach (var destination in configuration.Destinations)
        {
            await messages.SaveAsync(new OutboxMessage
            {
                Type = messageType,
                Message = serialized,
                QueueUrl = destination.QueueUrl,
                MessageGroupId = destination.MessageGroupId
            });
        }
    }

    public void Dispose()
    {
    }
}
