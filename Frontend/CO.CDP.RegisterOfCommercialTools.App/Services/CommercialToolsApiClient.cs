using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class CommercialToolsApiClient(HttpClient httpClient) : ISearchService
{
    public async Task<(List<SearchResult> Results, int TotalCount)> SearchAsync(SearchModel searchModel, int pageNumber, int pageSize)
    {
        var requestDto = new SearchRequestDto
        {
            Keyword = searchModel.Keywords,
            Status = searchModel.Status.Any() ? string.Join(",", searchModel.Status.Where(s => !string.IsNullOrWhiteSpace(s))) : null,
            SortBy = searchModel.SortOrder,
            SubmissionDeadlineFrom = searchModel.SubmissionDeadlineFrom?.ToDateTime(TimeOnly.MinValue),
            SubmissionDeadlineTo = searchModel.SubmissionDeadlineTo?.ToDateTime(TimeOnly.MinValue),
            ContractStartDateFrom = searchModel.ContractStartDateFrom?.ToDateTime(TimeOnly.MinValue),
            ContractStartDateTo = searchModel.ContractStartDateTo?.ToDateTime(TimeOnly.MinValue),
            ContractEndDateFrom = searchModel.ContractEndDateFrom?.ToDateTime(TimeOnly.MinValue),
            ContractEndDateTo = searchModel.ContractEndDateTo?.ToDateTime(TimeOnly.MinValue),
            MinFees = searchModel.NoFees != null ? 0 : searchModel.FeeMin,
            MaxFees = searchModel.NoFees != null ? 0 : searchModel.FeeMax,
            AwardMethod = searchModel.AwardMethod,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var response = await httpClient.GetFromJsonAsync<SearchResponse>($"api/Search?{ToQueryString(requestDto)}");

        var results = response?.Results.Select(dto => MapToSearchResult(dto)).ToList() ?? [];
        var totalCount = response?.TotalCount ?? 0;
        return (results, totalCount);
    }

    private SearchResult MapToSearchResult(SearchResultDto dto)
    {
        return new SearchResult
        (
            Id: dto.Id,
            Title: dto.Title,
            Caption: dto.Description,
            CommercialTool: dto.Title,
            Status: (SearchResultStatus)Enum.Parse(typeof(SearchResultStatus), dto.Status.ToString()),
            MaximumFee: dto.Fees > 0 ? dto.Fees.ToString("C", new System.Globalization.CultureInfo("en-GB")) : "Unknown",
            OtherContractingAuthorityCanUse: dto.ReservedParticipation ?? "Unknown",
            SubmissionDeadline: dto.SubmissionDeadline?.ToShortDateString() ?? "Unknown",
            ContractDates: "Unknown",
            AwardMethod: dto.AwardMethod,
            Url: dto.Url
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