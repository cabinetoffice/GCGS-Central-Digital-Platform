namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public required string Action { get; set; }
    public string? PropertyName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public required string UserId { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
