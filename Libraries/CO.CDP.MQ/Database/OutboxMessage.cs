namespace CO.CDP.MQ.Database;

public class OutboxMessage
{
    public int Id { get; set; }
    public required string Type { get; init; }
    public required string Message { get; init; }
    public bool Published { get; set; } = false;
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}