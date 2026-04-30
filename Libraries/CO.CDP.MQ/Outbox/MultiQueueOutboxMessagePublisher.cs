using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CO.CDP.MQ.Outbox;

public record MultiQueueOutboxMessagePublisherConfiguration
{
    public IReadOnlyList<OutboxRoute> Routes { get; init; } = [];
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

        var matchingRoutes = configuration.Routes
            .Where(r => r.MessageType == messageType)
            .ToList();

        if (matchingRoutes.Count == 0)
        {
            throw new InvalidOperationException(
                $"No outbox route configured for message type '{messageType}'. " +
                $"Add an entry to Aws:OutboxRoutes in the service configuration.");
        }

        logger.LogDebug(
            "Publishing the `{TYPE}` message to {COUNT} route(s)",
            messageType,
            matchingRoutes.Count
        );

        foreach (var route in matchingRoutes)
        {
            await messages.SaveAsync(new OutboxMessage
            {
                Type = messageType,
                Message = serialized,
                QueueUrl = route.QueueUrl,
                MessageGroupId = route.MessageGroupId
            });
        }
    }

    public void Dispose()
    {
    }
}