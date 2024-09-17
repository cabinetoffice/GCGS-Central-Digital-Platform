using CO.CDP.EntityFrameworkCore.Timestamps;

namespace CO.CDP.MQ.Outbox;

public class OutboxMessage : IEntityDate
{
    public int Id { get; set; }
    public required string Type { get; init; }
    public required string Message { get; init; }
    public bool Published { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}