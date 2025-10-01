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

        var searchQuery = ParseKeywordsForOData(keywords);

        return new CommercialToolsQueryBuilder(_params
            .SetItem("$search", searchQuery)
            .SetItem("filter[tender.name]", keywords)
            .SetItem("filter[tender.techniques.frameworkAgreement.description]", keywords)
            .SetItem("filter[parties.identifier.id]", keywords)
            .SetItem("filter[parties.name]", keywords), _skip, _top);
    }

    private static string ParseKeywordsForOData(string keywords)
    {
        keywords = keywords.Trim();

        if (keywords.StartsWith("\"") && keywords.EndsWith("\""))
        {
            return keywords;
        }

        if (keywords.Contains(" + "))
        {
            var terms = keywords.Split(" + ", StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" AND ", terms.Select(term => term.Trim()));
        }

        return keywords;
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

    public ICommercialToolsQueryBuilder ContractLocation(string region) =>
        WithParameterIf(!string.IsNullOrWhiteSpace(region), "filter[tender.items.deliveryAddresses.region]", region);

    public ICommercialToolsQueryBuilder WithCpv(string cpv) =>
        WithParameterIf(!string.IsNullOrWhiteSpace(cpv), "filter[tender.items.additionalClassifications.id]", cpv);

    public ICommercialToolsQueryBuilder WithAwardMethod(string awardMethod)
    {
        if (string.IsNullOrWhiteSpace(awardMethod))
            return this;

        var filterValue = awardMethod.ToLowerInvariant() switch
        {
            "with-competition" => "open",
            "without-competition" => "direct",
            _ => awardMethod
        };

        return WithParameter("filter[tender.awardCriteria.method]", filterValue);
    }

    public ICommercialToolsQueryBuilder WithFrameworkType(string frameworkType) =>
        WithParameterIf(!string.IsNullOrWhiteSpace(frameworkType), "filter[tender.techniques.frameworkAgreement.type]",
            frameworkType);

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