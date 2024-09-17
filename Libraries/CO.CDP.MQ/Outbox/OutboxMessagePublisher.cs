using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CO.CDP.MQ.Outbox;

public delegate string Serializer(object message);

public delegate string TypeMapper(Type type);

public class OutboxMessagePublisher(
    IOutboxMessageRepository messages,
    Serializer serializer,
    TypeMapper typeMapper,
    ILogger<OutboxMessagePublisher> logger
) : IPublisher
{
    public OutboxMessagePublisher(IOutboxMessageRepository messages, ILogger<OutboxMessagePublisher> logger)
        : this(messages, o => JsonSerializer.Serialize(o), type => type.Name, logger)
    {
    }

    public async Task Publish<TM>(TM message) where TM : notnull
    {
        var messageType = typeMapper(typeof(TM));
        var serialized = serializer(message);

        logger.LogDebug("Publishing the `{TYPE}` message: `{MESSAGE}`", messageType, serialized);

        await messages.SaveAsync(new OutboxMessage
        {
            Type = messageType,
            Message = serialized
        });
    }

    public void Dispose()
    {
    }
}