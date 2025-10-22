using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.WebApi.Foundation;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public interface ICommercialToolsService
{
    Task<ApiResult<(IEnumerable<SearchResultDto> results, int totalCount)>> SearchCommercialToolsWithCount(string queryUrl);
}