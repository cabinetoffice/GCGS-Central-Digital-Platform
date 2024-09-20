using CO.CDP.MQ.Outbox;
using FluentAssertions;

namespace CO.CDP.MQ.Tests.Outbox;

public class OutboxMessageTypeMapperFactoryTest
{
    [Fact]
    public void ItDelegatesTypeMappingForNonOutboxMessages()
    {
        var typeMapper = OutboxMessageTypeMapperFactory.Create(o => o.GetType().Name);

        var typeName = typeMapper(new TestMessage());

        typeName.Should().Be("TestMessage");
    }

    [Fact]
    public void ItReturnsPreDefinedMessageType()
    {
        var typeMapper = OutboxMessageTypeMapperFactory.Create(o => o.GetType().Name);

        var typeName = typeMapper(new OutboxMessage
        {
            Message = "{\"Content\":\"Message 3\"}",
            Type = "PreDefinedTestMessage"
        });


        typeName.Should().Be("PreDefinedTestMessage");
    }

    private record TestMessage;
}