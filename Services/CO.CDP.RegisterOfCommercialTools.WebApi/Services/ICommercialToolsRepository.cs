using CO.CDP.RegisterOfCommercialTools.WebApi.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public interface ICommercialToolsRepository
{
    Task<IEnumerable<SearchResultDto>> SearchCommercialTools(string queryUrl);
    Task<int> GetCommercialToolsCount(string queryUrl);
    Task<SearchResultDto?> GetCommercialToolById(string id);
}