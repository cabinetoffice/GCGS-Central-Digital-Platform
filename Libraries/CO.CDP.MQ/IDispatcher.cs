namespace CO.CDP.MQ;

public interface IDispatcher : IDisposable
{
    void Subscribe<TM>(Func<TM, Task> subscriber) where TM : class;

    Task ExecuteAsync(CancellationToken cancellationToken = default);
}