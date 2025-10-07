using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class LocationCodeService(ICommercialToolsApiClient client, ILogger<LocationCodeService> logger) : ILocationCodeService, IHierarchicalCodeService<NutsCodeDto>
{
    public async Task<List<NutsCodeDto>> GetRootLocationCodesAsync()
    {
        return await client.GetRootNutsCodesAsync() ?? [];
    }

    public async Task<List<NutsCodeDto>> GetRootCodesAsync()
    {
        return await GetRootLocationCodesAsync();
    }

    public async Task<List<NutsCodeDto>> GetChildrenAsync(string parentCode)
    {
        return await client.GetNutsChildrenAsync(parentCode) ?? [];
    }

    public async Task<List<NutsCodeDto>> SearchAsync(string query)
    {
        var sanitisedQuery = query.Replace("\r", "").Replace("\n", "");
        logger.LogInformation("Location code search executed: Query='{Query}'", sanitisedQuery);
        var results = await client.SearchNutsCodesAsync(query) ?? [];
        logger.LogInformation("Location code search completed: Query='{Query}', ResultCount={ResultCount}", sanitisedQuery, results.Count);
        return results;
    }

    public async Task<NutsCodeDto?> GetByCodeAsync(string code)
    {
        var result = await client.GetNutsCodesAsync([code]);
        return result?.FirstOrDefault();
    }

    public async Task<List<NutsCodeDto>> GetByCodesAsync(List<string> codes)
    {
        logger.LogInformation("Location codes retrieved by codes: Codes=[{Codes}], Count={Count}",
            string.Join(", ", codes.Select(code => code.Replace("\r", "").Replace("\n", ""))), codes.Count);
        return await client.GetNutsCodesAsync(codes) ?? [];
    }

    public async Task<List<NutsCodeDto>> GetHierarchyAsync(string code)
    {
        return await client.GetNutsHierarchyAsync(code) ?? [];
    }
}