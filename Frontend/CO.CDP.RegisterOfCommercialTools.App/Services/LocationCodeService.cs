using CO.CDP.Functional;
using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class LocationCodeService(ICommercialToolsApiClient client, ILogger<LocationCodeService> logger) : ILocationCodeService, IHierarchicalCodeService<NutsCodeDto>
{
    public async Task<List<NutsCodeDto>> GetRootLocationCodesAsync()
    {
        var result = await client.GetRootNutsCodesAsync();
        return result.GetOrElse(_ => []);
    }

    public async Task<List<NutsCodeDto>> GetRootCodesAsync()
    {
        return await GetRootLocationCodesAsync();
    }

    public async Task<List<NutsCodeDto>> GetChildrenAsync(string parentCode)
    {
        var result = await client.GetNutsChildrenAsync(parentCode);
        return result.GetOrElse(_ => []);
    }

    public async Task<List<NutsCodeDto>> SearchAsync(string query)
    {
        var sanitisedQuery = query.Replace("\r", "").Replace("\n", "");
        logger.LogInformation("Location code search executed: Query='{Query}'", sanitisedQuery);
        var result = await client.SearchNutsCodesAsync(query);
        var codes = result.GetOrElse(_ => []);
        logger.LogInformation("Location code search completed: Query='{Query}', ResultCount={ResultCount}", sanitisedQuery, codes.Count);
        return codes;
    }

    public async Task<NutsCodeDto?> GetByCodeAsync(string code)
    {
        var result = await client.GetNutsCodesAsync([code]);
        var codes = result.GetOrElse(_ => []);
        return codes.FirstOrDefault();
    }

    public async Task<List<NutsCodeDto>> GetByCodesAsync(List<string> codes)
    {
        logger.LogInformation("Location codes retrieved by codes: Codes=[{Codes}], Count={Count}",
            string.Join(", ", codes.Select(code => code.Replace("\r", "").Replace("\n", ""))), codes.Count);
        var result = await client.GetNutsCodesAsync(codes);
        return result.GetOrElse(_ => []);
    }

    public async Task<List<NutsCodeDto>> GetHierarchyAsync(string code)
    {
        var result = await client.GetNutsHierarchyAsync(code);
        return result.GetOrElse(_ => []);
    }
}