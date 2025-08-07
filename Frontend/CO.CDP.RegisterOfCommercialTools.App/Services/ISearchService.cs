using CO.CDP.RegisterOfCommercialTools.App.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public interface ISearchService
{
    Task<(List<SearchResult> Results, int TotalCount)> SearchAsync(SearchModel searchModel, int pageNumber,
        int pageSize);
}