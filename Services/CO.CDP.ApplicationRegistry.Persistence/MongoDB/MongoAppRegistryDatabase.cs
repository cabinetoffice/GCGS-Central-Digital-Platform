using CO.CDP.ApplicationRegistry.Persistence.Entities;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.MongoDB;

/// <summary>
/// Singleton that wraps <see cref="IMongoDatabase"/> and exposes a typed
/// <see cref="IMongoCollection{T}"/> for each ApplicationRegistry collection.
/// Registers indexes on construction.
/// </summary>
public class MongoAppRegistryDatabase
{
    // ── Collection name constants ──────────────────────────────────────────
    public const string ApplicationsCollection        = "app_registry_applications";
    public const string OrganisationsCollection       = "app_registry_organisations";
    public const string UserAssignmentsCollection     = "app_registry_user_assignments";
    public const string FeatureFlagsCollection        = "app_registry_feature_flags";
    public const string AuditLogsCollection           = "app_registry_audit_logs";
    public const string AccessControlCollection       = "app_registry_access_control";
    public const string ReportCategoriesCollection    = "app_registry_report_categories";
    public const string CategoryAssignmentsCollection = "app_registry_report_category_assignments";

    // ── Typed collection references ────────────────────────────────────────
    public IMongoCollection<Application>               Applications        { get; }
    public IMongoCollection<Organisation>              Organisations       { get; }
    public IMongoCollection<UserApplicationAssignment> UserAssignments     { get; }
    public IMongoCollection<FeatureFlag>               FeatureFlags        { get; }
    public IMongoCollection<AuditLog>                  AuditLogs           { get; }
    public IMongoCollection<AccessControlEntry>        AccessControl       { get; }
    public IMongoCollection<ReportCategory>            ReportCategories    { get; }
    public IMongoCollection<ReportCategoryAssignment>  CategoryAssignments { get; }

    public MongoAppRegistryDatabase(IMongoDatabase database)
    {
        Applications        = database.GetCollection<Application>(ApplicationsCollection);
        Organisations       = database.GetCollection<Organisation>(OrganisationsCollection);
        UserAssignments     = database.GetCollection<UserApplicationAssignment>(UserAssignmentsCollection);
        FeatureFlags        = database.GetCollection<FeatureFlag>(FeatureFlagsCollection);
        AuditLogs           = database.GetCollection<AuditLog>(AuditLogsCollection);
        AccessControl       = database.GetCollection<AccessControlEntry>(AccessControlCollection);
        ReportCategories    = database.GetCollection<ReportCategory>(ReportCategoriesCollection);
        CategoryAssignments = database.GetCollection<ReportCategoryAssignment>(CategoryAssignmentsCollection);
        // EnsureIndexes is NOT called here — it is called from application startup
        // so that test environments start without a live MongoDB connection.
    }

    /// <summary>
    /// Creates all collection indexes. Called once from application startup
    /// (fire-and-forget, wrapped in try/catch) so startup never blocks even if
    /// MongoDB is temporarily unavailable.
    /// </summary>
    public void EnsureIndexes()
    {
        // Applications — unique clientId, compound on isActive + name for list queries
        Applications.Indexes.CreateMany([
            new CreateIndexModel<Application>(
                Builders<Application>.IndexKeys.Ascending(a => a.ClientId),
                new CreateIndexOptions { Unique = true, Name = "idx_application_clientId" }),
            new CreateIndexModel<Application>(
                Builders<Application>.IndexKeys.Ascending(a => a.IsActive).Ascending(a => a.Name),
                new CreateIndexOptions { Name = "idx_application_active_name" })
        ]);

        // Organisations — unique slug
        Organisations.Indexes.CreateMany([
            new CreateIndexModel<Organisation>(
                Builders<Organisation>.IndexKeys.Ascending(o => o.Slug),
                new CreateIndexOptions { Unique = true, Name = "idx_organisation_slug" }),
            new CreateIndexModel<Organisation>(
                Builders<Organisation>.IndexKeys.Ascending("members.userPrincipalId"),
                new CreateIndexOptions { Name = "idx_organisation_members_upn" })
        ]);

        // UserAssignments — compound unique on (userPrincipalId, applicationId, organisationId)
        UserAssignments.Indexes.CreateMany([
            new CreateIndexModel<UserApplicationAssignment>(
                Builders<UserApplicationAssignment>.IndexKeys
                    .Ascending(a => a.UserPrincipalId)
                    .Ascending(a => a.ApplicationId)
                    .Ascending(a => a.OrganisationId),
                new CreateIndexOptions { Unique = true, Name = "idx_userassignment_unique" })
        ]);

        // FeatureFlags — unique tileId
        FeatureFlags.Indexes.CreateMany([
            new CreateIndexModel<FeatureFlag>(
                Builders<FeatureFlag>.IndexKeys.Ascending(f => f.TileId),
                new CreateIndexOptions { Unique = true, Name = "idx_featureflag_tileId" })
        ]);

        // AuditLogs — entityType + timestamp for filtered queries
        AuditLogs.Indexes.CreateMany([
            new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys.Ascending(a => a.EntityType).Descending(a => a.Timestamp),
                new CreateIndexOptions { Name = "idx_auditlog_entitytype_ts" })
        ]);
    }
}
