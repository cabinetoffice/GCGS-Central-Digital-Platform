using CO.CDP.Functional;
using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.WebApi.Foundation;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public interface ISearchService
{
    Task<Result<ApiError, (List<SearchResult> Results, int TotalCount)>> SearchAsync(SearchModel searchModel, int pageNumber,
        int pageSize);
}