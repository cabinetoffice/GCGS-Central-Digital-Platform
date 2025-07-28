namespace CO.CDP.RegisterOfCommercialTools.WebApi.Models;

public class SearchResponse
{
    public IEnumerable<SearchResultDto> Results { get; set; } = Enumerable.Empty<SearchResultDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}