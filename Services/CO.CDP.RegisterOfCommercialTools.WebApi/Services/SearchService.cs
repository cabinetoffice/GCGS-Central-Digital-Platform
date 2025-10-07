using System.Globalization;
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
            .WithKeywords(request.Keyword ?? string.Empty)
            .WithSkip(skip)
            .WithTop(top);
            // .WithOrderBy(request.SortBy ?? "relevance"); // Temporarily disabled until API supports it

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statuses = request.Status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var statusFilters = statuses.Select(s => MapStatusToODataFilter(s.Trim())).ToList();

            if (statusFilters.Count > 0)
            {
                var combinedStatusFilter = statusFilters.Count == 1
                    ? statusFilters[0]
                    : $"({string.Join(" or ", statusFilters)})";
                queryBuilder = queryBuilder.WithCustomFilter(combinedStatusFilter);
            }
        }

        queryBuilder = BuildFeeFilter(request.MinFees, request.MaxFees) switch
        {
            null => queryBuilder,
            var filter => queryBuilder.WithCustomFilter(filter)
        };

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

        if (request.AwardMethod != null && request.AwardMethod.Count > 0)
        {
            if (request.AwardMethod.Count == 2 &&
                request.AwardMethod.Contains("with-competition") &&
                request.AwardMethod.Contains("without-competition"))
            {
                queryBuilder = queryBuilder.WithAwardMethod("with-and-without-competition");
            }
            else if (request.AwardMethod.Count == 1)
            {
                queryBuilder = queryBuilder.WithAwardMethod(request.AwardMethod[0]);
            }
        }

        queryBuilder = BuildCpvFilter(request.CpvCodes) switch
        {
            null => queryBuilder,
            var filter => queryBuilder.WithCustomFilter(filter)
        };

        queryBuilder = BuildLocationFilter(request.LocationCodes) switch
        {
            null => queryBuilder,
            var filter => queryBuilder.WithCustomFilter(filter)
        };

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

    private static string MapStatusToODataFilter(string status) =>
        status.ToLowerInvariant() switch
        {
            "upcoming" => "(tender/status eq 'planned' or tender/status eq 'planning')",
            "active" => "tender/status eq 'active'",
            "active-buyers" => "(tender/status eq 'active' and tender/techniques/frameworkAgreement/type eq 'open')",
            "active-suppliers" => "(tender/status eq 'active' and (tender/techniques/frameworkAgreement/isOpenFrameworkScheme eq true or tender/techniques/hasDynamicPurchasingSystem eq true))",
            "awarded" => "(tender/status eq 'awarded' or tender/status eq 'complete')",
            "expired" => "(tender/status eq 'withdrawn' or tender/status eq 'cancelled')",
            _ => $"tender/status eq '{status}'"
        };

    private static string? BuildCpvFilter(List<string>? codes)
    {
        var validCodes = (codes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .ToList();

        return validCodes.Count switch
        {
            0 => null,
            1 => $"(tender/classification/scheme eq 'CPV' and tender/classification/classificationId eq '{validCodes[0]}')",
            _ => BuildMultipleCpvFilter(validCodes)
        };
    }

    private static string BuildMultipleCpvFilter(List<string> codes) =>
        $"(tender/classification/scheme eq 'CPV' and ({string.Join(" or ", codes.Select(c => $"tender/classification/classificationId eq '{c}'"))}))";

    private static string? BuildLocationFilter(List<string>? codes)
    {
        var validCodes = (codes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .ToList();

        return validCodes.Count switch
        {
            0 => null,
            1 => $"tender/items/any(i: i/deliveryAddresses/any(d: d/region eq '{validCodes[0]}'))",
            _ => BuildMultipleLocationFilter(validCodes)
        };
    }

    private static string BuildMultipleLocationFilter(List<string> codes) =>
        $"tender/items/any(i: i/deliveryAddresses/any(d: {string.Join(" or ", codes.Select(c => $"d/region eq '{c}'"))}))";

    private static string? BuildFeeFilter(decimal? minProportion, decimal? maxProportion)
    {
        if (minProportion == 0 && maxProportion == 0)
        {
            return "tender/participationFees/all(fee: fee/relativeValueProportion eq 0)";
        }

        var filters = new List<string>();

        if (minProportion.HasValue && minProportion > 0)
        {
            filters.Add($"tender/participationFees/any(fee: fee/relativeValueProportion ge {minProportion.Value.ToString(CultureInfo.InvariantCulture)})");
        }

        if (maxProportion.HasValue && maxProportion > 0)
        {
            filters.Add($"tender/participationFees/any(fee: fee/relativeValueProportion le {maxProportion.Value.ToString(CultureInfo.InvariantCulture)})");
        }

        return filters.Count > 0 ? string.Join(" and ", filters) : null;
    }
}