using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public interface ICommercialToolsService
{
    Task<(IEnumerable<SearchResultDto> results, int totalCount)> SearchCommercialToolsWithCount(string queryUrl);
}