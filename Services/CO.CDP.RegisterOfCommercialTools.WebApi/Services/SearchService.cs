using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class SearchService(
    ICommercialToolsQueryBuilder builder,
    ICommercialToolsService service,
    IConfiguration configuration)
    : ISearchService
{
    private readonly string _odataBaseUrl = configuration.GetSection("ODataApi:BaseUrl").Value ??
                                            throw new InvalidOperationException(
                                                "ODataApi:BaseUrl configuration is required");

    public async Task<SearchResponse> Search(SearchRequestDto request)
    {
        const int fixedPageSize = 20;

        var skip = request.Skip ?? (request.PageNumber - 1) * fixedPageSize;
        var top = request.Top ?? fixedPageSize;
        var pageNumber = (skip / fixedPageSize) + 1;

        var queryBuilder = builder
            .WithKeywords(request.Keyword ?? string.Empty)
            .WithStatus(request.Status ?? string.Empty)
            .WithSkip(skip)
            .WithTop(top);

        if (request.MinFees.HasValue || request.MaxFees.HasValue)
        {
            var feeFilter = FeeConverter.CreateProportionFilter(request.MinFees, request.MaxFees);
            if (!string.IsNullOrEmpty(feeFilter))
            {
                queryBuilder = queryBuilder.WithCustomFilter(feeFilter);
            }
        }

        if (request.SubmissionDeadlineFrom.HasValue)
            queryBuilder = queryBuilder.SubmissionDeadlineFrom(request.SubmissionDeadlineFrom.Value);

        if (request.SubmissionDeadlineTo.HasValue)
            queryBuilder = queryBuilder.SubmissionDeadlineTo(request.SubmissionDeadlineTo.Value);

        if (request.ContractStartDateFrom.HasValue)
            queryBuilder = queryBuilder.ContractStartDateFrom(request.ContractStartDateFrom.Value);

        if (request.ContractStartDateTo.HasValue)
            queryBuilder = queryBuilder.ContractStartDateTo(request.ContractStartDateTo.Value);

        if (request.ContractEndDateFrom.HasValue)
            queryBuilder = queryBuilder.ContractEndDateFrom(request.ContractEndDateFrom.Value);

        if (request.ContractEndDateTo.HasValue)
            queryBuilder = queryBuilder.ContractEndDateTo(request.ContractEndDateTo.Value);

        if (!string.IsNullOrWhiteSpace(request.FrameworkOptions))
        {
            queryBuilder = request.FrameworkOptions.ToLowerInvariant() switch
            {
                "open" => queryBuilder.OnlyOpenFrameworks(),
                "exclude-open" => queryBuilder.OnlyOpenFrameworks(false),
                _ => queryBuilder
            };
        }

        if (!string.IsNullOrWhiteSpace(request.DynamicMarketOptions))
        {
            switch (request.DynamicMarketOptions.ToLowerInvariant())
            {
                case "utilities-only":
                    queryBuilder = queryBuilder.WithBuyerClassificationRestrictions("utilities");
                    break;
                case "exclude-utilities":
                    queryBuilder = queryBuilder.ExcludeBuyerClassificationRestrictions("utilities");
                    break;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.ContractingAuthorityUsage))
        {
            var frameworkType = request.ContractingAuthorityUsage.ToLowerInvariant() == "yes" ? "open" : "closed";
            queryBuilder = queryBuilder.WithFrameworkType(frameworkType);
        }

        if (!string.IsNullOrWhiteSpace(request.AwardMethod))
        {
            queryBuilder = queryBuilder.WithAwardMethod(request.AwardMethod);
        }

        var queryUrl = queryBuilder.Build($"{_odataBaseUrl}/concepts/CommercialTools");

        var (results, totalCount) = await service.SearchCommercialToolsWithCount(queryUrl);

        return new SearchResponse
        {
            Results = results,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = top
        };
    }

}