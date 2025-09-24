using Microsoft.EntityFrameworkCore;

namespace CO.CDP.RegisterOfCommercialTools.Persistence;

public class DatabaseNutsCodeRepository(RegisterOfCommercialToolsContext context) : INutsCodeRepository
{
    public async Task<List<NutsCode>> GetRootCodesAsync(Culture culture = Culture.English)
    {
        var customOrder = new Dictionary<string, int>
        {
            { "UK", 1 },
            { "GG", 2 },
            { "IM", 3 },
            { "JE", 4 },
            { "BOT", 5 },
            { "ROW", 6 },
            { "NONE", 7 }
        };

        var rootCodes = await context.NutsCodes
            .AsNoTracking()
            .Where(c => c.ParentCode == null && c.IsActive)
            .Select(c => new NutsCode
            {
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionCy = c.DescriptionCy,
                ParentCode = c.ParentCode,
                Level = c.Level,
                IsActive = c.IsActive,
                IsSelectable = c.IsSelectable,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                HasChildren = context.NutsCodes.Any(child => child.ParentCode == c.Code && child.IsActive)
            })
            .ToListAsync();

        return rootCodes
            .OrderBy(c => customOrder.ContainsKey(c.Code) ? customOrder[c.Code] : 999)
            .ThenBy(c => culture == Culture.Welsh ? c.DescriptionCy : c.DescriptionEn)
            .ToList();
    }

    public async Task<List<NutsCode>> GetChildrenAsync(string parentCode, Culture culture = Culture.English)
    {
        return await context.NutsCodes
            .AsNoTracking()
            .Where(c => c.ParentCode == parentCode && c.IsActive)
            .Select(c => new NutsCode
            {
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionCy = c.DescriptionCy,
                ParentCode = c.ParentCode,
                Level = c.Level,
                IsActive = c.IsActive,
                IsSelectable = c.IsSelectable,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                HasChildren = context.NutsCodes.Any(child => child.ParentCode == c.Code && child.IsActive)
            })
            .OrderBy(c => culture == Culture.Welsh ? c.DescriptionCy : c.DescriptionEn)
            .ToListAsync();
    }

    public async Task<List<NutsCode>> SearchAsync(string query, Culture culture = Culture.English)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var searchTerm = query.ToLowerInvariant().Trim();

        var results = await context.NutsCodes
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsSelectable && (
                EF.Functions.TrigramsSimilarity(c.DescriptionEn.ToLower(), searchTerm) > 0.1 ||
                EF.Functions.TrigramsSimilarity(c.DescriptionCy.ToLower(), searchTerm) > 0.1 ||
                EF.Functions.TrigramsSimilarity(c.Code.ToLower(), searchTerm) > 0.1))
            .Select(c => new NutsCode
            {
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionCy = c.DescriptionCy,
                ParentCode = c.ParentCode,
                ParentDescriptionEn = c.ParentCode != null ?
                    context.NutsCodes
                        .Where(p => p.Code == c.ParentCode)
                        .Select(p => p.DescriptionEn)
                        .FirstOrDefault() : null,
                ParentDescriptionCy = c.ParentCode != null ?
                    context.NutsCodes
                        .Where(p => p.Code == c.ParentCode)
                        .Select(p => p.DescriptionCy)
                        .FirstOrDefault() : null,
                Level = c.Level,
                IsActive = c.IsActive,
                IsSelectable = c.IsSelectable,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                HasChildren = context.NutsCodes.Any(child => child.ParentCode == c.Code && child.IsActive)
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

    public async Task<NutsCode?> GetByCodeAsync(string code)
    {
        return await context.NutsCodes
            .AsNoTracking()
            .Select(c => new NutsCode
            {
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionCy = c.DescriptionCy,
                ParentCode = c.ParentCode,
                Level = c.Level,
                IsActive = c.IsActive,
                IsSelectable = c.IsSelectable,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                HasChildren = context.NutsCodes.Any(child => child.ParentCode == c.Code && child.IsActive)
            })
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
    }

    public async Task<List<NutsCode>> GetByCodesAsync(List<string> codes)
    {
        return await context.NutsCodes
            .AsNoTracking()
            .Where(c => codes.Contains(c.Code) && c.IsActive)
            .Select(c => new NutsCode
            {
                Code = c.Code,
                DescriptionEn = c.DescriptionEn,
                DescriptionCy = c.DescriptionCy,
                ParentCode = c.ParentCode,
                ParentDescriptionEn = c.ParentCode != null ?
                    context.NutsCodes
                        .Where(p => p.Code == c.ParentCode)
                        .Select(p => p.DescriptionEn)
                        .FirstOrDefault() : null,
                ParentDescriptionCy = c.ParentCode != null ?
                    context.NutsCodes
                        .Where(p => p.Code == c.ParentCode)
                        .Select(p => p.DescriptionCy)
                        .FirstOrDefault() : null,
                Level = c.Level,
                IsActive = c.IsActive,
                IsSelectable = c.IsSelectable,
                CreatedOn = c.CreatedOn,
                UpdatedOn = c.UpdatedOn,
                HasChildren = context.NutsCodes.Any(child => child.ParentCode == c.Code && child.IsActive)
            })
            .OrderBy(c => c.DescriptionEn)
            .ToListAsync();
    }

    public async Task<List<NutsCode>> GetHierarchyAsync(string code)
    {
        var hierarchy = new List<NutsCode>();
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

    public async Task<List<NutsCode>> GetAllAsync(Culture culture = Culture.English)
    {
        return await context.NutsCodes
            .Where(c => c.IsActive)
            .OrderBy(c => culture == Culture.Welsh ? c.DescriptionCy : c.DescriptionEn)
            .ToListAsync();
    }
}