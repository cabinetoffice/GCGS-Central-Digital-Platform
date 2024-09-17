namespace CO.CDP.MQ.Outbox;

public static class OutboxMessageSerializerFactory
{
    public static Func<object, string> Create(Func<object, string> serializer) =>
        o => o is OutboxMessage outboxMessage ? outboxMessage.Message : serializer(o);
}