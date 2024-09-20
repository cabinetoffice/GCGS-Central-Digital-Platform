namespace CO.CDP.MQ.Outbox;

public static class OutboxMessageTypeMapperFactory
{
    public static Func<object, string> Create(Func<object, string> typeMapper) =>
        o => o is OutboxMessage outboxMessage ? outboxMessage.Type : typeMapper(o);
}