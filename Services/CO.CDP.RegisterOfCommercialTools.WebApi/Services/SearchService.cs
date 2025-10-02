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
            .WithSkip(skip)
            .WithTop(top);

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
}