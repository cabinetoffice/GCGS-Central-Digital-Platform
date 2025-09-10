using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public interface ISearchService
{
    Task<SearchResponse> Search(SearchRequestDto request);
}