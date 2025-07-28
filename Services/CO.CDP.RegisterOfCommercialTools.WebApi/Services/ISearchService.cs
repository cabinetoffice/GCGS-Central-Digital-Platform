using CO.CDP.RegisterOfCommercialTools.WebApi.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public interface ISearchService
{
    Task<SearchResponse> Search(SearchRequestDto request);
    Task<SearchResultDto?> GetById(string id);
}