using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.WebApi.Foundation;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public interface ISearchService
{
    Task<ApiResult<SearchResponse>> Search(SearchRequestDto request);
}