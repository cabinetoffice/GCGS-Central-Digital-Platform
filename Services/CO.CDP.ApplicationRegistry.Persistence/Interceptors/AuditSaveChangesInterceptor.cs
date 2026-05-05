using CO.CDP.ApplicationRegistry.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CO.CDP.ApplicationRegistry.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that automatically logs all entity changes to the AuditLog table.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly string _currentUserId;

    public AuditSaveChangesInterceptor(string currentUserId = "system")
    {
        _currentUserId = currentUserId;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is ApplicationRegistryContext context)
        {
            var auditEntries = CreateAuditEntries(context.ChangeTracker);
            context.AuditLogs.AddRange(auditEntries);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> CreateAuditEntries(ChangeTracker changeTracker)
    {
        var auditEntries = new List<AuditLog>();

        foreach (var entry in changeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditLog))
        {
            var entityType = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry);
            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => "Unknown"
            };

            if (entry.State == EntityState.Modified)
            {
                foreach (var property in entry.Properties.Where(p => p.IsModified))
                {
                    auditEntries.Add(new AuditLog
                    {
                        EntityType = entityType,
                        EntityId = entityId,
                        Action = action,
                        PropertyName = property.Metadata.Name,
                        OldValue = property.OriginalValue?.ToString(),
                        NewValue = property.CurrentValue?.ToString(),
                        UserId = _currentUserId,
                        Timestamp = DateTimeOffset.UtcNow
                    });
                }
            }
            else
            {
                auditEntries.Add(new AuditLog
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    UserId = _currentUserId,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        return auditEntries;
    }

    private static Guid GetEntityId(EntityEntry entry)
    {
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        return idProperty?.CurrentValue is Guid id ? id : Guid.Empty;
    }
}
