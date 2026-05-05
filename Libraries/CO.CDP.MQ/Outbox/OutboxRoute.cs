namespace CO.CDP.MQ.Outbox;

public record OutboxRoute
{
    public required string MessageType { get; init; }
    public required string QueueUrl { get; init; }
    public required string MessageGroupId { get; init; }
}