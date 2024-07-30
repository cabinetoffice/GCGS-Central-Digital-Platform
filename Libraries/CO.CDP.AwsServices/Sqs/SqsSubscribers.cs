using CO.CDP.MQ;

namespace CO.CDP.AwsServices.Sqs;

public class SqsSubscribers
{
    private readonly Dictionary<Type, List<ISubscriber<object>>> _subscribers = [];

    public void Subscribe<TM>(ISubscriber<TM> subscriber) where TM : class
    {
        var subscribers = _subscribers.GetValueOrDefault(typeof(TM), []);
        subscribers.Add(AnySubscriber.From(subscriber));
        _subscribers[typeof(TM)] = subscribers;
    }

    public IEnumerable<ISubscriber<object>> AllMatching(Func<Type, bool> predicate)
    {
        return _subscribers.Where(kv => predicate(kv.Key)).SelectMany(kv => kv.Value);
    }
}

internal class AnySubscriber(Func<object, Task> f) : ISubscriber<object>
{
    public static AnySubscriber From<TEvent>(ISubscriber<TEvent> subscriber) where TEvent : class
    {
        return new AnySubscriber(o => subscriber.Handle((TEvent)o));
    }

    public Task Handle(object @event)
    {
        return f(@event);
    }
}