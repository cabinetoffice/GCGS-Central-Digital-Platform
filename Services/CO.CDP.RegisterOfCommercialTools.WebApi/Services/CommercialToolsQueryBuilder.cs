using System.Collections.Immutable;
using System.Globalization;

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

    public ICommercialToolsQueryBuilder WithKeywords(string keywords)
    {
        if (string.IsNullOrWhiteSpace(keywords))
            return this;

        var filter = BuildKeywordFilter(keywords);
        return WithCustomFilter(filter);
    }

    private static string BuildKeywordFilter(string keywords)
    {
        keywords = keywords.Trim();

        if (keywords.StartsWith("\"") && keywords.EndsWith("\""))
        {
            var phrase = keywords.Trim('"');
            return BuildContainsFilterForPhrase(phrase);
        }

        if (keywords.Contains(" + "))
        {
            var terms = keywords.Split(" + ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var filters = terms.Select(term => BuildContainsFilterForPhrase(term)).ToList();
            return $"({string.Join(" and ", filters)})";
        }

        var words = keywords.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length > 1)
        {
            var filters = words.Select(word => BuildContainsFilterForPhrase(word)).ToList();
            return $"({string.Join(" or ", filters)})";
        }

        return BuildContainsFilterForPhrase(keywords);
    }

    private static string BuildContainsFilterForPhrase(string phrase)
    {
        var escapedPhrase = phrase.Replace("'", "''").ToLower();

        var containsFilters = new List<string>
        {
            $"contains(tolower(tender/title), '{escapedPhrase}')",
            $"contains(tolower(tender/description), '{escapedPhrase}')",
            $"contains(tolower(tender/techniques/frameworkAgreement/description), '{escapedPhrase}')",

            $"parties/any(p: contains(tolower(p/name), '{escapedPhrase}'))",
            $"parties/any(p: contains(tolower(p/identifier/id), '{escapedPhrase}'))"
        };

        containsFilters.Add($"additionalIdentifiers/any(ai: contains(tolower(ai/id), '{escapedPhrase}'))");

        return $"({string.Join(" or ", containsFilters)})";
    }

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
            "active-suppliers" => "(tender/status eq 'active' and (tender/techniques/frameworkAgreement/isOpenFrameworkScheme eq true or tender/techniques/hasDynamicPurchasingSystem eq true))",
            "awarded" => "(tender/status eq 'awarded' or tender/status eq 'complete')",
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

    public ICommercialToolsQueryBuilder ContractStartDateFrom(DateTime from) =>
        WithParameter("filter[tender.lots.contractPeriod.startDate.from]", from.ToString("yyyy-MM-dd"));

    public ICommercialToolsQueryBuilder ContractStartDateTo(DateTime to) =>
        WithParameter("filter[tender.lots.contractPeriod.startDate.to]", to.ToString("yyyy-MM-dd"));

    public ICommercialToolsQueryBuilder ContractEndDateFrom(DateTime from) =>
        WithParameter("filter[tender.lots.contractPeriod.endDate.from]", from.ToString("yyyy-MM-dd"));

    public ICommercialToolsQueryBuilder ContractEndDateTo(DateTime to) =>
        WithParameter("filter[tender.lots.contractPeriod.endDate.to]", to.ToString("yyyy-MM-dd"));

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

    public ICommercialToolsQueryBuilder WithBuyerClassificationRestrictions(string restrictionId) =>
        WithParameterIf(!string.IsNullOrWhiteSpace(restrictionId),
            "filter[tender.techniques.frameworkAgreement.buyerClassificationRestrictions.id]", restrictionId);

    public ICommercialToolsQueryBuilder ExcludeBuyerClassificationRestrictions(string restrictionId) =>
        WithParameterIf(!string.IsNullOrWhiteSpace(restrictionId),
            "filter[tender.techniques.frameworkAgreement.buyerClassificationRestrictions.id ne]", restrictionId);

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
            "relevance" => "tender/status desc,tender/tenderPeriod/endDate asc,tender/title asc",
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
            return $"{baseUrl}{separator}queryOptions={queryOptionsValue}";
        }

        return baseUrl;
    }
}