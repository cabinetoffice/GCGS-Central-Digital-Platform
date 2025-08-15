using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient;

public interface ICommercialToolsApiClient
{
    Task<SearchResponse?> SearchAsync(SearchRequestDto request);
}