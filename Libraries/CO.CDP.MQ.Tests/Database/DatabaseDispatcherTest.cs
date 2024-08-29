using CO.CDP.MQ.Database;
using Moq;

namespace CO.CDP.MQ.Tests.Database;

public class DatabaseDispatcherTest
{
    [Fact]
    public void ItForwardsSubscriptionsToAnotherDispatcher()
    {
        var repository = new Mock<IOutboxMessageRepository>();
        var actualDispatcher = new Mock<IDispatcher>();
        var dispatcher = new DatabaseDispatcher(actualDispatcher.Object, repository.Object);
        var subscriber = new TestSubscriber<string>();

        dispatcher.Subscribe(subscriber);

        actualDispatcher.Verify(d => d.Subscribe(subscriber), Times.Once);
    }
}

class DatabaseDispatcher(IDispatcher dispatcher, IOutboxMessageRepository messages) : IDispatcher
{
    public void Dispose()
    {
    }

    public void Subscribe<TM>(ISubscriber<TM> subscriber) where TM : class
    {
        dispatcher.Subscribe(subscriber);
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}