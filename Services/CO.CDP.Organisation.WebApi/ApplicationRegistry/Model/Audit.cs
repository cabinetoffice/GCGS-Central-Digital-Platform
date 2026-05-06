namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record AuditLogDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    string? PropertyName,
    string? OldValue,
    string? NewValue,
    string UserId,
    DateTimeOffset Timestamp);
