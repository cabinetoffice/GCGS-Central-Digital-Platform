namespace CO.CDP.Organisation.WebApi.UseCase;

public interface IAuditLogService
{
    Task LogCreatedAsync(string entityType, int entityId, string userId);
    Task LogUpdatedAsync(string entityType, int entityId, string propertyName,
        string? oldValue, string? newValue, string userId);
    Task LogDeletedAsync(string entityType, int entityId, string userId);
    Task LogGrantedAsync(string entityType, int entityId,
        string? detail, string userId);
    Task LogRevokedAsync(string entityType, int entityId,
        string? detail, string userId);
}
