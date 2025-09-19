using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class CpvCodeService(ICommercialToolsApiClient client) : ICpvCodeService
{
    public async Task<List<CpvCodeDto>> GetRootCpvCodesAsync()
    {
        return await client.GetRootCpvCodesAsync() ?? [];
    }

    public async Task<List<CpvCodeDto>> GetChildrenAsync(string parentCode)
    {
        return await client.GetCpvChildrenAsync(parentCode) ?? [];
    }

    public async Task<List<CpvCodeDto>> SearchAsync(string query)
    {
        return await client.SearchCpvCodesAsync(query) ?? [];
    }

    public async Task<CpvCodeDto?> GetByCodeAsync(string code)
    {
        var result = await client.GetCpvCodesAsync([code]);
        return result?.FirstOrDefault();
    }

    public async Task<List<CpvCodeDto>> GetByCodesAsync(List<string> codes)
    {
        return await client.GetCpvCodesAsync(codes) ?? [];
    }

    public async Task<List<CpvCodeDto>> GetHierarchyAsync(string code)
    {
        return await client.GetCpvHierarchyAsync(code) ?? [];
    }
}