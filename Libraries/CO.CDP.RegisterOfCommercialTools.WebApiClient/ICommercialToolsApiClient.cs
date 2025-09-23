using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient;

public interface ICommercialToolsApiClient
{
    Task<SearchResponse?> SearchAsync(SearchRequestDto request);
    Task<List<CpvCodeDto>?> GetRootCpvCodesAsync(Culture culture = Culture.English);
    Task<List<CpvCodeDto>?> GetCpvChildrenAsync(string parentCode, Culture culture = Culture.English);
    Task<List<CpvCodeDto>?> SearchCpvCodesAsync(string query, Culture culture = Culture.English);
    Task<List<CpvCodeDto>?> GetCpvCodesAsync(List<string> codes);
    Task<List<CpvCodeDto>?> GetCpvHierarchyAsync(string code);
}