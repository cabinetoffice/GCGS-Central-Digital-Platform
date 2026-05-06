using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public interface IAuditRepository
{
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        string? entityType = null,
        string? action = null,
        string? userId = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        int limit = 100,
        int offset = 0);
}
