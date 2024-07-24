namespace CO.CDP.AwsServices.Sqs;

public class SqsSubscribers
{
    private readonly Dictionary<Type, List<Func<object, Task>>> _subscribers = [];

    public void Subscribe<TM>(Func<TM, Task> subscriber) where TM : class
    {
        var subscribers = _subscribers.GetValueOrDefault(typeof(TM), []);
        subscribers.Add(o => subscriber((TM)o));
        _subscribers[typeof(TM)] = subscribers;
    }

    public IEnumerable<Func<object, Task>> AllMatching(Func<Type, bool> predicate)
    {
        return _subscribers.Where(kv => predicate(kv.Key)).SelectMany(kv => kv.Value);
    }
}