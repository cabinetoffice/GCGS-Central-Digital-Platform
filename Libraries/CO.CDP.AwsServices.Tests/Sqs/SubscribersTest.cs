using CO.CDP.AwsServices.Sqs;
using CO.CDP.MQ;
using FluentAssertions;

namespace CO.CDP.AwsServices.Tests.Sqs;

public class SubscribersTest
{
    [Fact]
    public void ItReturnsAllSubscriberMatchingThePredicate()
    {
        var subscribers = new Subscribers();
        subscribers.Subscribe(new TestSubscriber<Foo>());
        subscribers.Subscribe(new TestSubscriber<Foo>());
        subscribers.Subscribe(new TestSubscriber<Bar>());

        subscribers.AllMatching(type => type == typeof(Foo))
            .Should().HaveCount(2);
        subscribers.AllMatching(type => type == typeof(Bar))
            .Should().HaveCount(1);
    }

    private class Foo;

    private class Bar;

    private class TestSubscriber<TEvent> : ISubscriber<TEvent> where TEvent : class
    {
        public Task Handle(TEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}