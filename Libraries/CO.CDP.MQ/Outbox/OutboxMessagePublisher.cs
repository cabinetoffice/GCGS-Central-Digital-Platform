using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static CO.CDP.MQ.Hosting.OutboxProcessorBackgroundService;

namespace CO.CDP.MQ.Outbox;

public delegate string Serializer(object message);

public delegate string TypeMapper(object type);

public record SqsPublisherConfiguration
{
    public required string QueueUrl { get; init; }
    public required string? MessageGroupId { get; init; }
    public OutboxProcessorConfiguration? Outbox { get; init; }
}


public class OutboxMessagePublisher(
    IOutboxMessageRepository messages,
    Serializer serializer,
    TypeMapper typeMapper,
    ILogger<OutboxMessagePublisher> logger
) : IPublisher
{
    public OutboxMessagePublisher(IOutboxMessageRepository messages, ILogger<OutboxMessagePublisher> logger)
        : this(messages, o => JsonSerializer.Serialize(o), type => type.GetType().Name, logger)
    {
    }

    public async Task Publish<TM>(TM message) where TM : notnull
    {
        var messageType = typeMapper(message);
        var serialized = serializer(message);

        logger.LogDebug("Publishing the `{TYPE}` message: `{MESSAGE}`", messageType, serialized);

        await messages.SaveAsync(new OutboxMessage
        {
            Type = messageType,
            Message = serialized,
            QueueUrl = "",
            MessageGroupId = "",
        });
    }

    public void Dispose()
    {
    }
}