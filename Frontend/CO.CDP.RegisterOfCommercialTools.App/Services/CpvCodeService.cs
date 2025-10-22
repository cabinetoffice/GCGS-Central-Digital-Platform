using CO.CDP.Functional;
using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class CpvCodeService(ICommercialToolsApiClient client, ILogger<CpvCodeService> logger) : ICpvCodeService, IHierarchicalCodeService<CpvCodeDto>
{
    public async Task<List<CpvCodeDto>> GetRootCpvCodesAsync()
    {
        var result = await client.GetRootCpvCodesAsync();
        return result.GetOrElse(_ => []);
    }

    public async Task<List<CpvCodeDto>> GetRootCodesAsync()
    {
        return await GetRootCpvCodesAsync();
    }

    public async Task<List<CpvCodeDto>> GetChildrenAsync(string parentCode)
    {
        var result = await client.GetCpvChildrenAsync(parentCode);
        return result.GetOrElse(_ => []);
    }

    public async Task<List<CpvCodeDto>> SearchAsync(string query)
    {
        var sanitisedQuery = query.Replace("\r", "").Replace("\n", "");
        logger.LogInformation("CPV code search executed: Query='{Query}'", sanitisedQuery);
        var result = await client.SearchCpvCodesAsync(query);
        var codes = result.GetOrElse(_ => []);
        logger.LogInformation("CPV code search completed: Query='{Query}', ResultCount={ResultCount}", sanitisedQuery, codes.Count);
        return codes;
    }

    public async Task<CpvCodeDto?> GetByCodeAsync(string code)
    {
        var result = await client.GetCpvCodesAsync([code]);
        var codes = result.GetOrElse(_ => []);
        return codes.FirstOrDefault();
    }

    public async Task<List<CpvCodeDto>> GetByCodesAsync(List<string> codes)
    {
        logger.LogInformation("CPV codes retrieved by codes: Codes=[{Codes}], Count={Count}",
            string.Join(", ", codes.Select(code => code.Replace("\r", "").Replace("\n", ""))), codes.Count);
        var result = await client.GetCpvCodesAsync(codes);
        return result.GetOrElse(_ => []);
    }

    public async Task<List<CpvCodeDto>> GetHierarchyAsync(string code)
    {
        var result = await client.GetCpvHierarchyAsync(code);
        return result.GetOrElse(_ => []);
    }
}