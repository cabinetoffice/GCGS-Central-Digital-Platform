using CO.CDP.RegisterOfCommercialTools.WebApi.Helpers;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.WebApi.Foundation;

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

    public async Task<ApiResult<SearchResponse>> Search(SearchRequestDto request)
    {
        LogSearchRequest(request);

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
            var statuses = request.Status
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            queryBuilder = queryBuilder.WithStatuses(statuses);
        }

        if (request.MinFees.HasValue && request.MinFees.Value == 0 && request.MaxFees.HasValue &&
            request.MaxFees.Value == 0)
        {
            queryBuilder =
                queryBuilder.WithCustomFilter("tender/participationFees/all(fee: fee/relativeValueProportion eq 0)");
        }
        else
        {
            if (request.MinFees.HasValue && request.MinFees.Value > 0)
                queryBuilder = queryBuilder.FeeFrom(request.MinFees.Value);

            if (request.MaxFees.HasValue && request.MaxFees.Value > 0)
                queryBuilder = queryBuilder.FeeTo(request.MaxFees.Value);
        }

        if (request.SubmissionDeadlineFrom.HasValue)
            queryBuilder = queryBuilder.SubmissionDeadlineFrom(request.SubmissionDeadlineFrom.Value);

        if (request.SubmissionDeadlineTo.HasValue)
            queryBuilder = queryBuilder.SubmissionDeadlineTo(request.SubmissionDeadlineTo.Value);

        if (request.ContractStartDate.HasValue)
            queryBuilder = queryBuilder.ContractStartDate(request.ContractStartDate.Value);

        if (request.ContractEndDate.HasValue)
            queryBuilder = queryBuilder.ContractEndDate(request.ContractEndDate.Value);

        if (request.FilterFrameworks && request.FilterDynamicMarkets)
        {
            queryBuilder = queryBuilder.WithCustomFilter(
                "(tender/techniques/hasFrameworkAgreement eq true or tender/techniques/hasDynamicPurchasingSystem eq true)");
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
                "open" => queryBuilder.WithFrameworkType("open"),
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

        if (request.ContractingAuthorityUsage != null && request.ContractingAuthorityUsage.Count > 0)
        {
            var usageValues = request.ContractingAuthorityUsage
                .Select(v => v.ToLowerInvariant())
                .ToList();

            if (usageValues.Count == 1)
            {
                var frameworkType = usageValues[0] == "yes" ? "open" : "closed";
                queryBuilder = queryBuilder.WithFrameworkType(frameworkType);
            }
        }

        queryBuilder = queryBuilder.WithAwardMethods(request.AwardMethod);

        queryBuilder = queryBuilder.WithCpvCodes(request.CpvCodes);

        queryBuilder = queryBuilder.WithLocationCodes(request.LocationCodes);

        var queryUrl = queryBuilder.Build($"{_odataBaseUrl}/concepts/CommercialTools");

        logger.LogInformation("Executing OData query: {QueryUrl}", queryUrl);

        var searchResult = await service.SearchCommercialToolsWithCount(queryUrl);

        return searchResult.Match(
            error => ApiResult<SearchResponse>.Failure(error),
            success =>
            {
                var (results, totalCount) = success;
                var searchResultDtos = results.ToList();

                logger.LogInformation(
                    "Search completed: ResultCount={ResultCount}, TotalCount={TotalCount}, PageNumber={PageNumber}",
                    searchResultDtos.Count, totalCount, pageNumber);

                var response = new SearchResponse
                {
                    Results = searchResultDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = top
                };

                return ApiResult<SearchResponse>.Success(response);
            }
        );
    }

    private void LogSearchRequest(SearchRequestDto request)
    {
        logger.LogInformation(
            "Search request received: PageNumber={PageNumber}, Keywords=[{Keywords}], SearchMode={SearchMode}, Status={Status}, " +
            "CpvCodes=[{CpvCodes}], LocationCodes=[{LocationCodes}], FilterFrameworks={FilterFrameworks}, FilterDynamicMarkets={FilterDynamicMarkets}, " +
            "AwardMethods=[{AwardMethods}], ContractingAuthorityUsage=[{ContractingAuthorityUsage}]",
            request.PageNumber,
            LogSanitizer.SanitizeList(request.Keywords),
            request.SearchMode,
            LogSanitizer.Sanitize(request.Status),
            LogSanitizer.SanitizeList(request.CpvCodes),
            LogSanitizer.SanitizeList(request.LocationCodes),
            request.FilterFrameworks,
            request.FilterDynamicMarkets,
            LogSanitizer.SanitizeList(request.AwardMethod),
            LogSanitizer.SanitizeList(request.ContractingAuthorityUsage));
    }
}