using CO.CDP.Functional;
using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.WebApi.Foundation;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class SearchService(ICommercialToolsApiClient commercialToolsApiClient) : ISearchService
{
    public async Task<Result<ApiError, (List<SearchResult> Results, int TotalCount)>> SearchAsync(
        SearchModel searchModel, int pageNumber, int pageSize)
    {
        var (keywords, searchMode) = ParseKeywords(searchModel.Keywords);

        var requestDto = new SearchRequestDto
        {
            Keywords = keywords,
            SearchMode = searchMode,
            Status = searchModel.Status.Any()
                ? string.Join(",", searchModel.Status.Where(s => !string.IsNullOrWhiteSpace(s)))
                : null,
            SortBy = searchModel.SortOrder,
            SubmissionDeadlineFrom = searchModel.SubmissionDeadlineFrom?.ToDateTime(TimeOnly.MinValue),
            SubmissionDeadlineTo = searchModel.SubmissionDeadlineTo?.ToDateTime(TimeOnly.MinValue),
            ContractStartDate = searchModel.ContractStartDateFrom?.ToDateTime(TimeOnly.MinValue),
            ContractEndDate = searchModel.ContractStartDateTo?.ToDateTime(TimeOnly.MinValue),
            MinFees = searchModel.NoFees != null ? 0 : searchModel.FeeMin / 100,
            MaxFees = searchModel.NoFees != null ? 0 : searchModel.FeeMax / 100,
            AwardMethod = searchModel.AwardMethod.Any() ? searchModel.AwardMethod : null,
            FilterFrameworks = searchModel.FilterFrameworks,
            IsOpenFrameworks = searchModel.IsOpenFrameworks,
            FilterDynamicMarkets = searchModel.FilterDynamicMarkets,
            IsUtilitiesOnly = searchModel.IsUtilitiesOnly,
            CpvCodes = searchModel.CpvCodes.Any() ? searchModel.CpvCodes : null,
            LocationCodes = searchModel.LocationCodes.Any() ? searchModel.LocationCodes : null,
            ContractingAuthorityUsage = searchModel.ContractingAuthorityUsage.Any()
                ? searchModel.ContractingAuthorityUsage
                : null,
            PageNumber = pageNumber
        };

        var apiResult = await commercialToolsApiClient.SearchAsync(requestDto);

        return apiResult.Match(
            error => error is ClientError
                ? Result<ApiError, (List<SearchResult>, int)>.Success((new List<SearchResult>(), 0))
                : Result<ApiError, (List<SearchResult>, int)>.Failure(error),
            searchResponse =>
            {
                var results = searchResponse.Results.Select(MapToSearchResult).ToList();
                return Result<ApiError, (List<SearchResult>, int)>.Success((results, searchResponse.TotalCount));
            }
        );
    }

    private static (List<string>? keywords, KeywordSearchMode searchMode) ParseKeywords(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (null, KeywordSearchMode.Any);

        var trimmed = input.Trim();

        if (trimmed.StartsWith("\"") && trimmed.EndsWith("\""))
        {
            var phrase = trimmed.Trim('"');
            return ([phrase], KeywordSearchMode.Exact);
        }

        if (trimmed.Contains('+'))
        {
            var terms = trimmed.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
            return (terms, KeywordSearchMode.All);
        }

        var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .ToList();
        return (words.Count > 0 ? words : null, KeywordSearchMode.Any);
    }

    private static SearchResult MapToSearchResult(SearchResultDto dto)
    {
        return new SearchResult
        (
            Id: dto.Id,
            Title: dto.Title,
            Caption: dto.BuyerName,
            CommercialTool: dto.CommercialTool,
            Status: dto.Status,
            MaximumFee: dto.MaximumFee,
            OtherContractingAuthorityCanUse: dto.OtherContractingAuthorityCanUse,
            SubmissionDeadline: dto.SubmissionDeadline,
            ContractDates: dto.ContractDates,
            AwardMethod: dto.AwardMethod,
            Url: dto.Url
        );
    }
}