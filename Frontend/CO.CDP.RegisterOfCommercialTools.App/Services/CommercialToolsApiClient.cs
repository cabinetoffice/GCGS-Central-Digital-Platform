using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class CommercialToolsApiClient : ISearchService
{
    private readonly HttpClient _httpClient;

    public CommercialToolsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(List<SearchResult> Results, int TotalCount)> SearchAsync(SearchModel searchModel, int pageNumber, int pageSize)
    {
        var requestDto = new SearchRequestDto
        {
            Keyword = searchModel.Keywords,
            Status = searchModel.CommercialToolStatus,
            SubmissionDeadlineFrom = searchModel.SubmissionDeadlineFrom?.ToDateTime(TimeOnly.MinValue),
            SubmissionDeadlineTo = searchModel.SubmissionDeadlineTo?.ToDateTime(TimeOnly.MinValue),
            ContractStartDateFrom = searchModel.ContractStartDateFrom?.ToDateTime(TimeOnly.MinValue),
            ContractStartDateTo = searchModel.ContractStartDateTo?.ToDateTime(TimeOnly.MinValue),
            ContractEndDateFrom = searchModel.ContractEndDateFrom?.ToDateTime(TimeOnly.MinValue),
            ContractEndDateTo = searchModel.ContractEndDateTo?.ToDateTime(TimeOnly.MinValue),
            MinFees = searchModel.NoFees != null ? 0 : searchModel.FeeFrom,
            MaxFees = searchModel.NoFees != null ? 0 : searchModel.FeeTo,
            AwardMethod = searchModel.AwardMethod,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var response = await _httpClient.GetFromJsonAsync<SearchResponse>($"api/Search?{ToQueryString(requestDto)}");

        var results = response?.Results?.Select(MapToSearchResult).ToList() ?? new List<SearchResult>();
        var totalCount = response?.TotalCount ?? 0;
        return (results, totalCount);
    }

    public async Task<SearchResult?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<SearchResultDto>($"api/Search/{id}");
            return response == null ? null : MapToSearchResult(response);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            return null;
        }
    }

    private static SearchResult MapToSearchResult(SearchResultDto dto)
    {
        return new SearchResult
        (
            Id: dto.Id,
            Title: dto.Title,
            Caption: dto.Description,
            CommercialTool: dto.Title,
            Status: (SearchResultStatus)Enum.Parse(typeof(SearchResultStatus), dto.Status.ToString()),
            MaximumFee: dto.Fees.ToString("C"),
            OtherContractingAuthorityCanUse: dto.ReservedParticipation ?? "N/A",
            SubmissionDeadline: dto.SubmissionDeadline?.ToShortDateString() ?? "N/A",
            ContractDates: "N/A",
            AwardMethod: dto.AwardMethod
        );
    }

    private static string ToQueryString(SearchRequestDto dto)
    {
        var properties = from p in dto.GetType().GetProperties()
                         where p.GetValue(dto, null) != null
                         select $"{p.Name}={Uri.EscapeDataString(p.GetValue(dto, null)!.ToString()!)}";

        return string.Join("&", properties);
    }
}