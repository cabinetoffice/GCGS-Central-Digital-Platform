using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class CpvCodeService(ICommercialToolsApiClient client, ILogger<CpvCodeService> logger) : ICpvCodeService, IHierarchicalCodeService<CpvCodeDto>
{
    public async Task<List<CpvCodeDto>> GetRootCpvCodesAsync()
    {
        return await client.GetRootCpvCodesAsync() ?? [];
    }

    public async Task<List<CpvCodeDto>> GetRootCodesAsync()
    {
        return await GetRootCpvCodesAsync();
    }

    public async Task<List<CpvCodeDto>> GetChildrenAsync(string parentCode)
    {
        return await client.GetCpvChildrenAsync(parentCode) ?? [];
    }

    public async Task<List<CpvCodeDto>> SearchAsync(string query)
    {
        var sanitisedQuery = query.Replace("\r", "").Replace("\n", "");
        logger.LogInformation("CPV code search executed: Query='{Query}'", sanitisedQuery);
        var results = await client.SearchCpvCodesAsync(query) ?? [];
        logger.LogInformation("CPV code search completed: Query='{Query}', ResultCount={ResultCount}", sanitisedQuery, results.Count);
        return results;
    }

    public async Task<CpvCodeDto?> GetByCodeAsync(string code)
    {
        var result = await client.GetCpvCodesAsync([code]);
        return result?.FirstOrDefault();
    }

    public async Task<List<CpvCodeDto>> GetByCodesAsync(List<string> codes)
    {
        logger.LogInformation("CPV codes retrieved by codes: Codes=[{Codes}], Count={Count}",
            string.Join(", ", codes.Select(code => code.Replace("\r", "").Replace("\n", ""))), codes.Count);
        return await client.GetCpvCodesAsync(codes) ?? [];
    }

    public async Task<List<CpvCodeDto>> GetHierarchyAsync(string code)
    {
        return await client.GetCpvHierarchyAsync(code) ?? [];
    }
}