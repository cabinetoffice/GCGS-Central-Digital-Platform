namespace CO.CDP.MQ;

public interface IDispatcher : IDisposable
{
    void Subscribe<TM>(ISubscriber<TM> subscriber) where TM : class;

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}