using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.WebApi.Foundation;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient;

public interface ICommercialToolsApiClient
{
    Task<ApiResult<SearchResponse>> SearchAsync(SearchRequestDto request);
    Task<ApiResult<List<CpvCodeDto>>> GetRootCpvCodesAsync(Culture culture = Culture.English);
    Task<ApiResult<List<CpvCodeDto>>> GetCpvChildrenAsync(string parentCode, Culture culture = Culture.English);
    Task<ApiResult<List<CpvCodeDto>>> SearchCpvCodesAsync(string query, Culture culture = Culture.English);
    Task<ApiResult<List<CpvCodeDto>>> GetCpvCodesAsync(List<string> codes);
    Task<ApiResult<List<CpvCodeDto>>> GetCpvHierarchyAsync(string code);

    Task<ApiResult<List<NutsCodeDto>>> GetRootNutsCodesAsync(Culture culture = Culture.English);
    Task<ApiResult<List<NutsCodeDto>>> GetNutsChildrenAsync(string parentCode, Culture culture = Culture.English);
    Task<ApiResult<List<NutsCodeDto>>> SearchNutsCodesAsync(string query, Culture culture = Culture.English);
    Task<ApiResult<List<NutsCodeDto>>> GetNutsCodesAsync(List<string> codes);
    Task<ApiResult<List<NutsCodeDto>>> GetNutsHierarchyAsync(string code);
}