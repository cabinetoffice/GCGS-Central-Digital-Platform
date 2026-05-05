using CO.CDP.ApplicationRegistry.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.ApplicationRegistry.Persistence.Repositories;

public class DatabaseCategoryRepository : ICategoryRepository
{
    private readonly ApplicationRegistryContext _context;

    public DatabaseCategoryRepository(ApplicationRegistryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReportCategory>> GetAllAsync()
    {
        return await _context.ReportCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ReportCategory?> GetByIdAsync(Guid id)
    {
        return await _context.ReportCategories
            .Include(c => c.Permissions)
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ReportCategory> CreateAsync(ReportCategory category)
    {
        _context.ReportCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(ReportCategory category)
    {
        _context.ReportCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CategoryPermission>> GetPermissionsAsync(Guid categoryId)
    {
        return await _context.CategoryPermissions
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task SetPermissionsAsync(Guid categoryId, IEnumerable<CategoryPermission> permissions)
    {
        var existing = await _context.CategoryPermissions
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();

        _context.CategoryPermissions.RemoveRange(existing);
        _context.CategoryPermissions.AddRange(permissions);
        await _context.SaveChangesAsync();
    }

    public async Task ClearPermissionsAsync(Guid categoryId)
    {
        var existing = await _context.CategoryPermissions
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();

        _context.CategoryPermissions.RemoveRange(existing);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ReportCategoryAssignment>> GetReportCategoriesAsync(Guid reportId)
    {
        return await _context.ReportCategoryAssignments
            .Include(a => a.Category)
            .Where(a => a.ReportId == reportId)
            .ToListAsync();
    }

    public async Task AssignReportToCategoryAsync(ReportCategoryAssignment assignment)
    {
        _context.ReportCategoryAssignments.Add(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveReportFromCategoryAsync(Guid reportId, Guid categoryId)
    {
        var assignment = await _context.ReportCategoryAssignments
            .FirstOrDefaultAsync(a => a.ReportId == reportId && a.CategoryId == categoryId);

        if (assignment != null)
        {
            _context.ReportCategoryAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }
}
