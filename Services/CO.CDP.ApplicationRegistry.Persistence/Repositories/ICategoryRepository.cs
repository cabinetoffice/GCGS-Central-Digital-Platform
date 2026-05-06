using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<ReportCategory>> GetAllAsync();
    Task<ReportCategory?> GetByIdAsync(Guid id);
    Task<ReportCategory> CreateAsync(ReportCategory category);
    Task UpdateAsync(ReportCategory category);
    Task<IEnumerable<CategoryPermission>> GetPermissionsAsync(Guid categoryId);
    Task SetPermissionsAsync(Guid categoryId, IEnumerable<CategoryPermission> permissions);
    Task ClearPermissionsAsync(Guid categoryId);
    Task<IEnumerable<ReportCategoryAssignment>> GetReportCategoriesAsync(Guid reportId);
    Task AssignReportToCategoryAsync(ReportCategoryAssignment assignment);
    Task RemoveReportFromCategoryAsync(Guid reportId, Guid categoryId);
}
