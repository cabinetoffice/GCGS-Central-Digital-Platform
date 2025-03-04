using System.Text.Json;
using CO.CDP.MQ.Outbox;
using FluentAssertions;

namespace CO.CDP.MQ.Tests.Outbox;

public class OutboxMessageSerializerFactoryTest
{
    [Fact]
    public void ItDelegatesSerializationForNonOutboxMessages()
    {
        var serializer = OutboxMessageSerializerFactory.Create(o => JsonSerializer.Serialize(o));

        var serialized = serializer(new TestMessage("Message 1"));

        serialized.Should().Be("{\"Content\":\"Message 1\"}");
    }

    [Fact]
    public void ItReturnsPreSerializedMessage()
    {
        var serializer = OutboxMessageSerializerFactory.Create(o => JsonSerializer.Serialize(o));

        var serialized = serializer(new OutboxMessage
        {
            Message = "{\"Content\":\"Message 2\"}",
            Type = "TestMessage",
            QueueUrl = "test-queue",
            MessageGroupId = "test-messages"
        });

        serialized.Should().Be("{\"Content\":\"Message 2\"}");
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record TestMessage(string Content);
}