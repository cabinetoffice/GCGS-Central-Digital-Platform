using System.Text.Json;
using CO.CDP.MQ.Outbox;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.MQ.Tests.Outbox;

public class MultiQueueOutboxMessagePublisherTest
{
    private readonly ILogger<MultiQueueOutboxMessagePublisher> _logger =
        LoggerFactory.Create(_ => { }).CreateLogger<MultiQueueOutboxMessagePublisher>();

    private readonly Mock<IOutboxMessageRepository> _repository = new();

    private MultiQueueOutboxMessagePublisher Publisher(MultiQueueOutboxMessagePublisherConfiguration config) =>
        new(
            _repository.Object,
            o => JsonSerializer.Serialize(o),
            o => o.GetType().Name,
            config,
            _logger
        );

    [Fact]
    public async Task ItPersistsOneMessagePerMatchingRoute()
    {
        var config = new MultiQueueOutboxMessagePublisherConfiguration
        {
            Routes =
            [
                new OutboxRoute { MessageType = "TestMessage", QueueUrl = "queue://a", MessageGroupId = "GroupA" },
                new OutboxRoute { MessageType = "TestMessage", QueueUrl = "queue://b", MessageGroupId = "GroupB" }
            ]
        };

        await Publisher(config).Publish(new TestMessage(1, "Hello"));

        _repository.Verify(r => r.SaveAsync(It.IsAny<OutboxMessage>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ItUsesTheMatchingRouteQueueUrlAndMessageGroupId()
    {
        var config = new MultiQueueOutboxMessagePublisherConfiguration
        {
            Routes =
            [
                new OutboxRoute
                    { MessageType = "TestMessage", QueueUrl = "queue://target", MessageGroupId = "TargetGroup" }
            ]
        };

        await Publisher(config).Publish(new TestMessage(2, "World"));

        _repository.Invocations.First().Arguments[0].Should().BeEquivalentTo(
            new OutboxMessage
            {
                Type = "TestMessage",
                Message = """{"Id":2,"Name":"World"}""",
                QueueUrl = "queue://target",
                MessageGroupId = "TargetGroup"
            }
        );
    }

    [Fact]
    public async Task ItIgnoresRoutesForUnrelatedMessageTypes()
    {
        var config = new MultiQueueOutboxMessagePublisherConfiguration
        {
            Routes =
            [
                new OutboxRoute { MessageType = "OtherMessage", QueueUrl = "queue://other", MessageGroupId = "Other" },
                new OutboxRoute
                    { MessageType = "TestMessage", QueueUrl = "queue://correct", MessageGroupId = "Correct" }
            ]
        };

        await Publisher(config).Publish(new TestMessage(3, "Filtered"));

        _repository.Verify(r => r.SaveAsync(It.IsAny<OutboxMessage>()), Times.Once);
        _repository.Invocations.First().Arguments[0].As<OutboxMessage>().QueueUrl.Should().Be("queue://correct");
    }

    [Fact]
    public async Task ItThrowsWhenNoRouteMatchesTheMessageType()
    {
        var config = new MultiQueueOutboxMessagePublisherConfiguration
        {
            Routes =
            [
                new OutboxRoute { MessageType = "OtherMessage", QueueUrl = "queue://other", MessageGroupId = "Other" }
            ]
        };

        var act = async () => await Publisher(config).Publish(new TestMessage(4, "NoRoute"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*TestMessage*");
    }

    [Fact]
    public async Task ItDoesNotSaveWhenNoRouteMatchesTheMessageType()
    {
        var config = new MultiQueueOutboxMessagePublisherConfiguration
        {
            Routes =
            [
                new OutboxRoute { MessageType = "OtherMessage", QueueUrl = "queue://other", MessageGroupId = "Other" }
            ]
        };

        try
        {
            await Publisher(config).Publish(new TestMessage(5, "NoSave"));
        }
        catch (InvalidOperationException)
        {
            // expected
        }

        _repository.Verify(r => r.SaveAsync(It.IsAny<OutboxMessage>()), Times.Never);
    }

    private record TestMessage(int Id, string Name);
}