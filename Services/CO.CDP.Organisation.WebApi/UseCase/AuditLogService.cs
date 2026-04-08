using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class AuditLogService(OrganisationInformationContext db) : IAuditLogService
{
    public async Task LogCreatedAsync(string entityType, int entityId, string userId)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Guid = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = "Created",
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task LogUpdatedAsync(string entityType, int entityId, string propertyName,
        string? oldValue, string? newValue, string userId)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Guid = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = "Updated",
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task LogDeletedAsync(string entityType, int entityId, string userId)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Guid = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = "Deleted",
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task LogGrantedAsync(string entityType, int entityId,
        string? detail, string userId)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Guid = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = "Granted",
            NewValue = detail,
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task LogRevokedAsync(string entityType, int entityId,
        string? detail, string userId)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Guid = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = "Revoked",
            NewValue = detail,
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
