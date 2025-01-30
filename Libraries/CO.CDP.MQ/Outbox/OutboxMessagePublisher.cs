using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static CO.CDP.MQ.Hosting.OutboxProcessorBackgroundService;
using static CO.CDP.MQ.Outbox.OutboxMessagePublisher;

namespace CO.CDP.MQ.Outbox;

public delegate string Serializer(object message);

public delegate string TypeMapper(object type);

public class OutboxMessagePublisher(
    IOutboxMessageRepository messages,
    Serializer serializer,
    TypeMapper typeMapper,
    OutboxMessagePublisherConfiguration configuration,
    ILogger<OutboxMessagePublisher> logger
) : IPublisher
{
    public OutboxMessagePublisher(
        IOutboxMessageRepository messages,
        OutboxMessagePublisherConfiguration configuration,
        ILogger<OutboxMessagePublisher> logger)
        : this(messages, o => JsonSerializer.Serialize(o), type => type.GetType().Name, configuration, logger)
    {
    }

    public record OutboxMessagePublisherConfiguration
    {
        public string QueueUrl { get; init; } = "";
        public string MessageGroupId { get; init; } = "";
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
            QueueUrl = configuration.QueueUrl,
            MessageGroupId = configuration.MessageGroupId
        });
    }

    public void Dispose()
    {
    }
}