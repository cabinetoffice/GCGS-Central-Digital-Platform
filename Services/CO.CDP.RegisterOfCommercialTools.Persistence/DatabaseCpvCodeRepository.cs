using Microsoft.EntityFrameworkCore;

namespace CO.CDP.RegisterOfCommercialTools.Persistence;

public class DatabaseCpvCodeRepository(RegisterOfCommercialToolsContext context) : ICpvCodeRepository
{
    public async Task<List<CpvCode>> GetRootCodesAsync(Culture culture = Culture.English)
    {
        return await context.CpvCodes
            .Where(c => c.ParentCode == null && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<List<CpvCode>> GetChildrenAsync(string parentCode, Culture culture = Culture.English)
    {
        return await context.CpvCodes
            .Where(c => c.ParentCode == parentCode && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<List<CpvCode>> SearchAsync(string query, Culture culture = Culture.English)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var searchTerms = query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return await context.CpvCodes
            .Where(c => c.IsActive && searchTerms.All(term =>
                c.Code.ToLower().Contains(term) ||
                c.DescriptionEn.ToLower().Contains(term) ||
                c.DescriptionCy.ToLower().Contains(term)))
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<CpvCode?> GetByCodeAsync(string code)
    {
        return await context.CpvCodes
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
    }

    public async Task<List<CpvCode>> GetByCodesAsync(List<string> codes)
    {
        return await context.CpvCodes
            .Where(c => codes.Contains(c.Code) && c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<List<CpvCode>> GetHierarchyAsync(string code)
    {
        var hierarchy = new List<CpvCode>();
        var current = await GetByCodeAsync(code);

        while (current != null)
        {
            hierarchy.Insert(0, current);

            if (current.ParentCode != null)
            {
                current = await GetByCodeAsync(current.ParentCode);
            }
            else
            {
                current = null;
            }
        }

        return hierarchy;
    }

    public async Task<List<CpvCode>> GetAllAsync(Culture culture = Culture.English)
    {
        return await context.CpvCodes
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }
}