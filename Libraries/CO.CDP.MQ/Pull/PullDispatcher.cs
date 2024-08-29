using Microsoft.Extensions.Logging;

namespace CO.CDP.MQ.Pull;

public delegate object Deserializer(string type, string body);

public delegate bool TypeMatcher(Type type, string typeName);

public abstract class PullDispatcher<TMessage>(
    Deserializer deserializer,
    TypeMatcher typeMatcher,
    ILogger<PullDispatcher<TMessage>> logger
) : IDispatcher
{
    private readonly Subscribers _subscribers = new();

    public virtual void Subscribe<TM>(ISubscriber<TM> subscriber) where TM : class
    {
        _subscribers.Subscribe(subscriber);
    }

    public virtual Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run((Func<Task?>)(async () =>
        {
            logger.LogInformation("Started the message dispatcher");

            while (!cancellationToken.IsCancellationRequested)
            {
                await HandleMessages(cancellationToken);
            }
        }), cancellationToken);
    }

    public void Dispose()
    {
    }

    private async Task HandleMessages(CancellationToken cancellationToken)
    {
        try
        {
            foreach (var message in await ReceiveMessages(cancellationToken))
            {
                await HandleMessage(message);
            }
        }
        catch (Exception cause)
        {
            logger.LogError(cause, "Failed to handle messages");
        }
    }

    private async Task HandleMessage(TMessage message)
    {
        try
        {
            await InvokeSubscribers(message);
            await DeleteMessage(message);
        }
        catch (Exception cause)
        {
            logger.LogError(cause, "Failed to handle the message with MessageId={MessageId}", MessageId(message));
        }
    }

    private async Task InvokeSubscribers(TMessage message)
    {
        logger.LogDebug("Handling the message with MessageId={MessageId}", MessageId(message));

        var type = MessageType(message);
        var matchingSubscribers = _subscribers.AllMatching((t) => typeMatcher(t, type)).ToList();

        logger.LogDebug("Found {CNT} subscribers to handle the `{TYPE}` message", matchingSubscribers.Count, type);

        if (matchingSubscribers.Any())
        {
            var deserialized = deserializer(type, MessageBody(message));

            logger.LogDebug("Handling the `{TYPE}` message: `{MESSAGE}`", type, MessageBody(message));

            foreach (var subscriber in matchingSubscribers)
            {
                await subscriber.Handle(deserialized);
            }
        }
    }

    protected abstract Task<List<TMessage>> ReceiveMessages(CancellationToken cancellationToken);
    protected abstract Task DeleteMessage(TMessage message);
    protected abstract string MessageId(TMessage message);
    protected abstract string MessageBody(TMessage message);
    protected abstract string MessageType(TMessage message);
}