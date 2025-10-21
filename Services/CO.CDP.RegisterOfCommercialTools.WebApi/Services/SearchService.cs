using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class SearchService(
    ICommercialToolsQueryBuilder builder,
    ICommercialToolsService service,
    IConfiguration configuration,
    ILogger<SearchService> logger)
    : ISearchService
{
    private readonly string _odataBaseUrl = configuration.GetSection("ODataApi:BaseUrl").Value ??
                                            throw new InvalidOperationException(
                                                "ODataApi:BaseUrl configuration is required");

    public async Task<SearchResponse> Search(SearchRequestDto request)
    {
        logger.LogInformation("Search request received: PageNumber={PageNumber}, CpvCodes={CpvCount}, LocationCodes={LocationCount}",
            request.PageNumber, request.CpvCodes?.Count ?? 0, request.LocationCodes?.Count ?? 0);

        const int fixedPageSize = 20;

        var skip = request.Skip ?? (request.PageNumber - 1) * fixedPageSize;
        var top = request.Top ?? fixedPageSize;
        var pageNumber = (skip / fixedPageSize) + 1;

        var queryBuilder = builder
            .WithKeywords(request.Keywords, request.SearchMode)
            .WithSkip(skip)
            .WithTop(top)
            .WithOrderBy(request.SortBy ?? "relevance");

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statuses = request.Status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            queryBuilder = queryBuilder.WithStatuses(statuses);
        }

        if (request.MinFees.HasValue && request.MinFees.Value == 0 && request.MaxFees.HasValue && request.MaxFees.Value == 0)
        {
            queryBuilder = queryBuilder.WithCustomFilter("tender/participationFees/all(fee: fee/relativeValueProportion eq 0)");
        }
        else
        {
            if (request.MinFees.HasValue && request.MinFees.Value > 0)
                queryBuilder = queryBuilder.FeeFrom(request.MinFees.Value * 100);

            if (request.MaxFees.HasValue && request.MaxFees.Value > 0)
                queryBuilder = queryBuilder.FeeTo(request.MaxFees.Value * 100);
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

        if (request.FilterFrameworks && request.FilterDynamicMarkets)
        {
            queryBuilder = queryBuilder.WithCustomFilter("(tender/techniques/hasFrameworkAgreement eq true or tender/techniques/hasDynamicPurchasingSystem eq true)");
        }
        else if (request.FilterFrameworks)
        {
            queryBuilder = queryBuilder.WithFrameworkAgreement();
        }
        else if (request.FilterDynamicMarkets)
        {
            queryBuilder = queryBuilder.WithDynamicPurchasingSystem();
        }

        if (request.IsOpenFrameworks)
            queryBuilder = queryBuilder.OnlyOpenFrameworks();

        if (request.IsUtilitiesOnly)
            queryBuilder = queryBuilder.WithBuyerClassificationRestrictions("utilities");

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
            if (request.DynamicMarketOptions.ToLowerInvariant() == "utilities-only")
            {
                queryBuilder = queryBuilder.WithBuyerClassificationRestrictions("utilities");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.ContractingAuthorityUsage))
        {
            var frameworkType = request.ContractingAuthorityUsage.ToLowerInvariant() == "yes" ? "open" : "closed";
            queryBuilder = queryBuilder.WithFrameworkType(frameworkType);
        }

        queryBuilder = queryBuilder.WithAwardMethods(request.AwardMethod);

        queryBuilder = queryBuilder.WithCpvCodes(request.CpvCodes);

        queryBuilder = queryBuilder.WithLocationCodes(request.LocationCodes);

        var queryUrl = queryBuilder.Build($"{_odataBaseUrl}/concepts/CommercialTools");

        logger.LogInformation("Executing OData query: {QueryUrl}", queryUrl);

        return await ExecuteSearchWithFallback(queryUrl, pageNumber, top);
    }

    private async Task<SearchResponse> ExecuteSearchWithFallback(string queryUrl, int pageNumber, int pageSize)
    {
        var (results, totalCount) = await service.SearchCommercialToolsWithCount(queryUrl);

        var searchResultDtos = results.ToList();
        logger.LogInformation("Search completed: ResultCount={ResultCount}, TotalCount={TotalCount}, PageNumber={PageNumber}",
            searchResultDtos.ToList().Count, totalCount, pageNumber);

        return new SearchResponse
        {
            Results = searchResultDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}