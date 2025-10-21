using System.Collections.Immutable;
using System.Globalization;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class CommercialToolsQueryBuilder : ICommercialToolsQueryBuilder
{
    private readonly ImmutableDictionary<string, string> _params;
    private readonly int? _skip;
    private readonly int? _top;

    public CommercialToolsQueryBuilder() : this(
        ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase))
    {
    }

    private CommercialToolsQueryBuilder(ImmutableDictionary<string, string> parameters, int? skip = null,
        int? top = null)
    {
        _params = parameters;
        _skip = skip;
        _top = top;
    }

    private ICommercialToolsQueryBuilder WithParameter(string key, string value) =>
        new CommercialToolsQueryBuilder(_params.SetItem(key, value), _skip, _top);

    private ICommercialToolsQueryBuilder WithParameterIf(bool condition, string key, string value) =>
        condition ? WithParameter(key, value) : this;

    public ICommercialToolsQueryBuilder WithKeywords(List<string>? keywords, KeywordSearchMode searchMode)
    {
        if (keywords == null || keywords.Count == 0)
            return this;

        var filter = BuildKeywordFilter(keywords, searchMode);
        return WithCustomFilter(filter);
    }

    private static string BuildKeywordFilter(List<string> keywords, KeywordSearchMode searchMode)
    {
        if (keywords.Count == 0)
            return string.Empty;

        if (keywords.Count == 1)
        {
            return BuildContainsFilterForPhrase(keywords[0]);
        }

        if (searchMode == KeywordSearchMode.Exact)
        {
            var phrase = string.Join(" ", keywords);
            return BuildContainsFilterForPhrase(phrase);
        }

        if (searchMode == KeywordSearchMode.Any)
        {
            return BuildAnyKeywordFilter(keywords);
        }

        var filters = keywords.Select(BuildContainsFilterForPhrase).ToList();
        return $"({string.Join(" and ", filters)})";
    }

    private static string BuildAnyKeywordFilter(List<string> keywords)
    {
        var fieldFilters = new List<string>();

        foreach (var keyword in keywords)
        {
            fieldFilters.AddRange(BuildFiltersForKeyword(keyword));
        }

        return $"({string.Join(" or ", fieldFilters)})";
    }

    private static string BuildContainsFilterForPhrase(string phrase)
    {
        var filters = BuildFiltersForKeyword(phrase);
        return $"({string.Join(" or ", filters)})";
    }

    private static List<string> BuildFiltersForKeyword(string keyword)
    {
        var escapedKeyword = keyword.Replace("'", "''").ToLower();
        var filters = new List<string>();

        if (IsOcid(keyword))
        {
            filters.Add($"contains(tolower(ocid), '{escapedKeyword}')");
        }
        else if (IsDocumentId(keyword))
        {
            filters.Add($"planning/documents/any(d: contains(tolower(d/documentId), '{escapedKeyword}'))");
            filters.Add($"tender/documents/any(d: contains(tolower(d/documentId), '{escapedKeyword}'))");
        }
        else if (IsIdentifier(keyword))
        {
            filters.Add($"parties/any(p: contains(tolower(p/identifier/id), '{escapedKeyword}'))");
            filters.Add($"additionalIdentifiers/any(ai: contains(tolower(ai/id), '{escapedKeyword}'))");
        }
        else
        {
            filters.Add($"contains(tolower(tender/title), '{escapedKeyword}')");
            filters.Add($"contains(tolower(tender/description), '{escapedKeyword}')");
            filters.Add($"contains(tolower(tender/techniques/frameworkAgreement/description), '{escapedKeyword}')");
            filters.Add($"parties/any(p: contains(tolower(p/name), '{escapedKeyword}'))");
        }

        return filters;
    }

    private static bool IsOcid(string keyword) =>
        keyword.StartsWith("ocds-", StringComparison.OrdinalIgnoreCase);

    private static bool IsDocumentId(string keyword) =>
        System.Text.RegularExpressions.Regex.IsMatch(keyword, @"^\d{6,}-\d{4}$");

    private static bool IsIdentifier(string keyword)
    {
        if (keyword.Length < 6)
            return false;

        var digitCount = keyword.Count(char.IsDigit);
        var letterCount = keyword.Count(char.IsLetter);
        var hyphenCount = keyword.Count(c => c == '-');

        return digitCount >= 4 && (digitCount + hyphenCount + letterCount) == keyword.Length;
    }

    public ICommercialToolsQueryBuilder WithFrameworkAgreement(bool hasFramework = true) =>
        WithCustomFilter($"tender/techniques/hasFrameworkAgreement eq {hasFramework.ToString().ToLower()}");

    public ICommercialToolsQueryBuilder WithDynamicPurchasingSystem(bool hasDps = true) =>
        WithCustomFilter($"tender/techniques/hasDynamicPurchasingSystem eq {hasDps.ToString().ToLower()}");

    public ICommercialToolsQueryBuilder OnlyOpenFrameworks(bool only = true) =>
        WithParameter("filter[tender.techniques.frameworkAgreement.isOpenFrameworkScheme]", only.ToString().ToLower());

    public ICommercialToolsQueryBuilder WithStatus(string status) =>
        string.IsNullOrWhiteSpace(status)
            ? this
            : WithCustomFilter(MapStatusToODataFilter(status.ToLowerInvariant()));

    private static string MapStatusToODataFilter(string status) =>
        status switch
        {
            "upcoming" => "(tender/status eq 'planned' or tender/status eq 'planning')",
            "active" => "tender/status eq 'active'",
            "active-buyers" => "(tender/status eq 'active' and tender/techniques/frameworkAgreement/type eq 'open')",
            "active-suppliers" => "(tender/status eq 'active' and (tender/techniques/frameworkAgreement/type eq 'open' or tender/techniques/hasDynamicPurchasingSystem eq true))",
            "awarded" => "tender/status eq 'complete'",
            "cancelled" => "tender/status eq 'cancelled'",
            "expired" => "(tender/status eq 'withdrawn' or tender/status eq 'cancelled')",
            _ => $"tender/status eq '{status}'"
        };

    public ICommercialToolsQueryBuilder FeeFrom(decimal from)
    {
        var proportion = from / 100;
        return WithParameter("filter[tender.participationFees.relativeValue.proportion.from]",
            proportion.ToString(CultureInfo.InvariantCulture));
    }

    public ICommercialToolsQueryBuilder FeeTo(decimal to)
    {
        var proportion = to / 100;
        return WithParameter("filter[tender.participationFees.relativeValue.proportion.to]",
            proportion.ToString(CultureInfo.InvariantCulture));
    }

    public ICommercialToolsQueryBuilder SubmissionDeadlineFrom(DateTime from) =>
        WithParameter("filter[tender.tenderPeriod.endDate.from]", from.ToString("yyyy-MM-dd"));

    public ICommercialToolsQueryBuilder SubmissionDeadlineTo(DateTime to) =>
        WithParameter("filter[tender.tenderPeriod.endDate.to]", to.ToString("yyyy-MM-dd"));

    public ICommercialToolsQueryBuilder ContractStartDateFrom(DateTime from)
    {
        var filter = $"(awards/any(a: a/contractPeriod/startDate ge {from:yyyy-MM-dd}) or tender/techniques/frameworkAgreement/periodStartDate ge {from:yyyy-MM-dd} or tender/lots/any(l: l/contractPeriod/startDate ge {from:yyyy-MM-dd}))";
        return WithCustomFilter(filter);
    }

    public ICommercialToolsQueryBuilder ContractStartDateTo(DateTime to)
    {
        var filter = $"(awards/any(a: a/contractPeriod/startDate le {to:yyyy-MM-dd}) or tender/techniques/frameworkAgreement/periodStartDate le {to:yyyy-MM-dd} or tender/lots/any(l: l/contractPeriod/startDate le {to:yyyy-MM-dd}))";
        return WithCustomFilter(filter);
    }

    public ICommercialToolsQueryBuilder ContractEndDateFrom(DateTime from)
    {
        var filter = $"(awards/any(a: a/contractPeriod/endDate ge {from:yyyy-MM-dd}) or tender/techniques/frameworkAgreement/periodEndDate ge {from:yyyy-MM-dd} or tender/lots/any(l: l/contractPeriod/endDate ge {from:yyyy-MM-dd}))";
        return WithCustomFilter(filter);
    }

    public ICommercialToolsQueryBuilder ContractEndDateTo(DateTime to)
    {
        var filter = $"(awards/any(a: a/contractPeriod/endDate le {to:yyyy-MM-dd}) or tender/techniques/frameworkAgreement/periodEndDate le {to:yyyy-MM-dd} or tender/lots/any(l: l/contractPeriod/endDate le {to:yyyy-MM-dd}))";
        return WithCustomFilter(filter);
    }

    public ICommercialToolsQueryBuilder WithLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return this;

        var odataFilter = $"tender/items/any(i: i/deliveryAddresses/any(d: d/region eq '{location}'))";
        return WithCustomFilter(odataFilter);
    }

    public ICommercialToolsQueryBuilder WithCpv(string cpv)
    {
        if (string.IsNullOrWhiteSpace(cpv))
            return this;

        var odataFilter = $"(tender/classification/scheme eq 'CPV' and tender/classification/classificationId eq '{cpv}')";
        return WithCustomFilter(odataFilter);
    }

    public ICommercialToolsQueryBuilder WithAwardMethod(string awardMethod)
    {
        if (string.IsNullOrWhiteSpace(awardMethod))
            return this;

        var odataFilter = awardMethod.ToLowerInvariant() switch
        {
            "with-competition" => "(tender/techniques/frameworkAgreement/method eq 'open' or tender/techniques/frameworkAgreement/method eq 'withReopeningCompetition' or tender/techniques/frameworkAgreement/method eq 'withAndWithoutReopeningCompetition')",
            "without-competition" => "(tender/techniques/frameworkAgreement/method eq 'direct' or tender/techniques/frameworkAgreement/method eq 'withoutReopeningCompetition' or tender/techniques/frameworkAgreement/method eq 'withAndWithoutReopeningCompetition')",
            "with-and-without-competition" => "(tender/techniques/frameworkAgreement/method eq 'open' or tender/techniques/frameworkAgreement/method eq 'withReopeningCompetition' or tender/techniques/frameworkAgreement/method eq 'withAndWithoutReopeningCompetition' or tender/techniques/frameworkAgreement/method eq 'direct' or tender/techniques/frameworkAgreement/method eq 'withoutReopeningCompetition')",
            _ => $"tender/techniques/frameworkAgreement/method eq '{awardMethod}'"
        };

        return WithCustomFilter(odataFilter);
    }

    public ICommercialToolsQueryBuilder WithFrameworkType(string frameworkType) =>
        string.IsNullOrWhiteSpace(frameworkType)
            ? this
            : WithCustomFilter($"tender/techniques/frameworkAgreement/type eq '{frameworkType}'");

    public ICommercialToolsQueryBuilder WithBuyerClassificationRestrictions(string restrictionId)
    {
        if (string.IsNullOrWhiteSpace(restrictionId))
            return this;

        var classificationId = restrictionId.ToLowerInvariant() == "utilities" ? "privateUtility" : restrictionId;
        var filter = $"parties/any(p: p/roles/any(r: r eq 'buyer') and p/detail/classifications/any(c: c/scheme eq 'UK_CA_TYPE' and c/classificationId eq '{classificationId}'))";
        return WithCustomFilter(filter);
    }

    public ICommercialToolsQueryBuilder WithSkip(int skip) =>
        new CommercialToolsQueryBuilder(_params, skip, _top);

    public ICommercialToolsQueryBuilder WithTop(int top) =>
        new CommercialToolsQueryBuilder(_params, _skip, top);

    public ICommercialToolsQueryBuilder WithCustomFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return this;

        var existingFilter = _params.GetValueOrDefault("$filter");
        var newFilter = string.IsNullOrEmpty(existingFilter)
            ? filter
            : $"({existingFilter}) and ({filter})";

        return new CommercialToolsQueryBuilder(_params.SetItem("$filter", newFilter), _skip, _top);
    }

    public ICommercialToolsQueryBuilder WithOrderBy(string sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return this;

        var orderByClause = sortBy.ToLowerInvariant() switch
        {
            "a-z" => "tender/title asc",
            "z-a" => "tender/title desc",
            "relevance" => "tender/createdDate desc",
            _ => null
        };

        if (orderByClause == null)
            return this;

        return WithParameter("$orderby", orderByClause);
    }

    public string Build(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl));

        var odataParams = new List<string>();

        if (_skip.HasValue)
        {
            odataParams.Add($"$skip={_skip.Value}");
        }

        if (_top.HasValue)
        {
            odataParams.Add($"$top={_top.Value}");
        }

        foreach (var param in _params)
        {
            odataParams.Add($"{param.Key}={Uri.EscapeDataString(param.Value)}");
        }

        if (odataParams.Any())
        {
            odataParams.Insert(0, "$count=true");

            var queryOptionsValue = string.Join("&", odataParams);
            var separator = baseUrl.Contains('?') ? '&' : '?';
            return $"{baseUrl}{separator}{queryOptionsValue}";
        }

        return baseUrl;
    }
}