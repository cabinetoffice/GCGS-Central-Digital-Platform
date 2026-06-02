using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.MongoDB;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories.MongoDB;

/// <summary>
/// MongoDB concrete implementation of <see cref="ICategoryRepository"/>.
///
/// Document model:
///   <c>app_registry_report_categories</c> — embeds <c>permissions[]</c> (CategoryPermission).
///   <c>app_registry_report_category_assignments</c> — flat join documents.
/// </summary>
public class MongoCategoryRepository : ICategoryRepository
{
    private readonly IMongoCollection<ReportCategory>           _categories;
    private readonly IMongoCollection<ReportCategoryAssignment> _assignments;
    private readonly IAuditRepository                           _audit;

    public MongoCategoryRepository(MongoAppRegistryDatabase db, IAuditRepository audit)
    {
        _categories  = db.ReportCategories;
        _assignments = db.CategoryAssignments;
        _audit       = audit;
    }

    // ── Categories ─────────────────────────────────────────────────────────

    public async Task<IEnumerable<ReportCategory>> GetAllAsync()
    {
        return await _categories
            .Find(c => c.IsActive)
            .SortBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ReportCategory?> GetByIdAsync(Guid id)
    {
        return await _categories
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<ReportCategory> CreateAsync(ReportCategory category)
    {
        await _categories.InsertOneAsync(category);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ReportCategory),
            EntityId   = category.Id,
            Action     = "Created",
            UserId     = "system"
        });
        return category;
    }

    public async Task UpdateAsync(ReportCategory category)
    {
        await _categories.ReplaceOneAsync(c => c.Id == category.Id, category);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ReportCategory),
            EntityId   = category.Id,
            Action     = "Updated",
            UserId     = "system"
        });
    }

    // ── Category Permissions (embedded) ───────────────────────────────────

    public async Task<IEnumerable<CategoryPermission>> GetPermissionsAsync(Guid categoryId)
    {
        var category = await _categories
            .Find(c => c.Id == categoryId)
            .FirstOrDefaultAsync();

        return category?.Permissions ?? [];
    }

    public async Task SetPermissionsAsync(Guid categoryId, IEnumerable<CategoryPermission> permissions)
    {
        var permList = permissions.ToList();

        var update = Builders<ReportCategory>.Update
            .Set(c => c.Permissions, permList);

        await _categories.UpdateOneAsync(c => c.Id == categoryId, update);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(CategoryPermission),
            EntityId   = categoryId,
            Action     = "PermissionsSet",
            UserId     = "system"
        });
    }

    public async Task ClearPermissionsAsync(Guid categoryId)
    {
        var update = Builders<ReportCategory>.Update
            .Set(c => c.Permissions, new List<CategoryPermission>());

        await _categories.UpdateOneAsync(c => c.Id == categoryId, update);
    }

    // ── Report–Category Assignments ────────────────────────────────────────

    public async Task<IEnumerable<ReportCategoryAssignment>> GetReportCategoriesAsync(Guid reportId)
    {
        var assignments = await _assignments
            .Find(a => a.ReportId == reportId)
            .ToListAsync();

        if (assignments.Count == 0) return [];

        // Hydrate Category navigation from the categories collection.
        var categoryIds = assignments.Select(a => a.CategoryId).ToList();
        var categories = await _categories
            .Find(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        var catMap = categories.ToDictionary(c => c.Id);

        foreach (var assignment in assignments)
        {
            if (catMap.TryGetValue(assignment.CategoryId, out var cat))
                assignment.Category = cat;
        }

        return assignments;
    }

    public async Task AssignReportToCategoryAsync(ReportCategoryAssignment assignment)
    {
        assignment.AssignedAt = DateTimeOffset.UtcNow;
        await _assignments.InsertOneAsync(assignment);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ReportCategoryAssignment),
            EntityId   = assignment.Id,
            Action     = "Created",
            UserId     = "system"
        });
    }

    public async Task RemoveReportFromCategoryAsync(Guid reportId, Guid categoryId)
    {
        var assignment = await _assignments
            .Find(a => a.ReportId == reportId && a.CategoryId == categoryId)
            .FirstOrDefaultAsync();

        if (assignment == null) return;

        await _assignments.DeleteOneAsync(a => a.Id == assignment.Id);
        await _audit.LogAsync(new AuditLog
        {
            EntityType = nameof(ReportCategoryAssignment),
            EntityId   = assignment.Id,
            Action     = "Deleted",
            UserId     = "system"
        });
    }
}
