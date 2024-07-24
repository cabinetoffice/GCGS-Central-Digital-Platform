using CO.CDP.AwsServices.Sqs;
using FluentAssertions;

namespace CO.CDP.AwsServices.Tests.Sqs;

public class SqsSubscribersTest
{
    [Fact]
    public void ItReturnsAllSubscriberMatchingThePredicate()
    {
        var subscribers = new SqsSubscribers();
        subscribers.Subscribe<Foo>(_ => Task.CompletedTask);
        subscribers.Subscribe<Foo>(_ => Task.CompletedTask);
        subscribers.Subscribe<Bar>(_ => Task.CompletedTask);

        subscribers.AllMatching(type => type == typeof(Foo))
            .Should().HaveCount(2);
        subscribers.AllMatching(type => type == typeof(Bar))
            .Should().HaveCount(1);
    }

    private class Foo;

    private class Bar;
}