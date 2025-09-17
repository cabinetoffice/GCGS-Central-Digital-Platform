using Microsoft.EntityFrameworkCore;

namespace CO.CDP.RegisterOfCommercialTools.Persistence;

public class DatabaseCpvCodeRepository(RegisterOfCommercialToolsContext context) : ICpvCodeRepository
{
    public async Task<List<CpvCode>> GetRootCodesAsync(Culture culture = Culture.English)
    {
        return await context.CpvCodes
            .AsNoTracking()
            .Where(c => c.ParentCode == null && c.IsActive)
            .Select(c => new CpvCode
            {
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionCy = c.DescriptionCy,
                ParentCode = c.ParentCode,
                Level = c.Level,
                IsActive = c.IsActive,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                HasChildren = context.CpvCodes.Any(child => child.ParentCode == c.Code && child.IsActive)
            })
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<List<CpvCode>> GetChildrenAsync(string parentCode, Culture culture = Culture.English)
    {
        return await context.CpvCodes
            .AsNoTracking()
            .Where(c => c.ParentCode == parentCode && c.IsActive)
            .Select(c => new CpvCode
            {
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionCy = c.DescriptionCy,
                ParentCode = c.ParentCode,
                Level = c.Level,
                IsActive = c.IsActive,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                HasChildren = context.CpvCodes.Any(child => child.ParentCode == c.Code && child.IsActive)
            })
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<List<CpvCode>> SearchAsync(string query, Culture culture = Culture.English)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var searchTerm = query.ToLowerInvariant().Trim();

        var results = await context.CpvCodes
            .AsNoTracking()
            .Where(c => c.IsActive && (
                EF.Functions.TrigramsSimilarity(c.DescriptionEn.ToLower(), searchTerm) > 0.1 ||
                EF.Functions.TrigramsSimilarity(c.DescriptionCy.ToLower(), searchTerm) > 0.1 ||
                EF.Functions.TrigramsSimilarity(c.Code.ToLower(), searchTerm) > 0.1))
            .Select(c => new CpvCode
            {
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionCy = c.DescriptionCy,
                ParentCode = c.ParentCode,
                Level = c.Level,
                IsActive = c.IsActive,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                HasChildren = context.CpvCodes.Any(child => child.ParentCode == c.Code && child.IsActive)
            })
            .OrderByDescending(c =>
                Math.Max(
                    Math.Max(
                        EF.Functions.TrigramsSimilarity(c.DescriptionEn.ToLower(), searchTerm),
                        EF.Functions.TrigramsSimilarity(c.DescriptionCy.ToLower(), searchTerm)
                    ),
                    EF.Functions.TrigramsSimilarity(c.Code.ToLower(), searchTerm)
                ))
            .Take(10)
            .ToListAsync();

        return results;
    }

    public async Task<CpvCode?> GetByCodeAsync(string code)
    {
        return await context.CpvCodes
            .AsNoTracking()
            .Select(c => new CpvCode
            {
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionCy = c.DescriptionCy,
                ParentCode = c.ParentCode,
                Level = c.Level,
                IsActive = c.IsActive,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                HasChildren = context.CpvCodes.Any(child => child.ParentCode == c.Code && child.IsActive)
            })
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
    }

    public async Task<List<CpvCode>> GetByCodesAsync(List<string> codes)
    {
        return await context.CpvCodes
            .AsNoTracking()
            .Where(c => codes.Contains(c.Code) && c.IsActive)
            .Select(c => new CpvCode
            {
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionCy = c.DescriptionCy,
                ParentCode = c.ParentCode,
                Level = c.Level,
                IsActive = c.IsActive,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                HasChildren = context.CpvCodes.Any(child => child.ParentCode == c.Code && child.IsActive)
            })
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