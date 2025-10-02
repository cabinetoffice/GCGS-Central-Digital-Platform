using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class SearchService(ICommercialToolsApiClient commercialToolsApiClient) : ISearchService
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
            MinFees = searchModel.NoFees != null ? 0 : searchModel.FeeMin / 100,
            MaxFees = searchModel.NoFees != null ? 0 : searchModel.FeeMax / 100,
            AwardMethod = searchModel.AwardMethod.Any() ? searchModel.AwardMethod : null,
            CpvCodes = searchModel.CpvCodes.Any() ? searchModel.CpvCodes : null,
            PageNumber = pageNumber
        };

        var response = await commercialToolsApiClient.SearchAsync(requestDto);

        var results = response?.Results.Select(MapToSearchResult).ToList() ?? [];
        var totalCount = response?.TotalCount ?? 0;
        return (results, totalCount);
    }

    private static SearchResult MapToSearchResult(SearchResultDto dto)
    {
        return new SearchResult
        (
            Id: dto.Id,
            Title: dto.Title,
            Caption: dto.Description,
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