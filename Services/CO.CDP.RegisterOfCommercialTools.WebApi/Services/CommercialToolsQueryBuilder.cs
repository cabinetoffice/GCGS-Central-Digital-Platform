using System.Collections.Immutable;
using System.Globalization;
using System.Text;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class CommercialToolsQueryBuilder : ICommercialToolsQueryBuilder
{
    private readonly ImmutableDictionary<string, string> _params;

    public CommercialToolsQueryBuilder() : this(
        ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase))
    {
    }

    private CommercialToolsQueryBuilder(ImmutableDictionary<string, string> parameters)
    {
        _params = parameters;
    }

    private ICommercialToolsQueryBuilder WithParameter(string key, string value) =>
        new CommercialToolsQueryBuilder(_params.SetItem(key, value));

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
            .SetItem("filter[parties.name]", keywords));
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
        WithParameterIf(!string.IsNullOrWhiteSpace(status), "filter[tender.status]", status);

    public ICommercialToolsQueryBuilder FeeFrom(decimal from) =>
        WithParameter("filter[tender.participationFees.relativeValue.proportion.value.from]",
            from.ToString(CultureInfo.InvariantCulture));

    public ICommercialToolsQueryBuilder FeeTo(decimal to) =>
        WithParameter("filter[tender.participationFees.relativeValue.proportion.value.to]",
            to.ToString(CultureInfo.InvariantCulture));

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

    public ICommercialToolsQueryBuilder WithFrameworkType(string frameworkType) =>
        WithParameterIf(!string.IsNullOrWhiteSpace(frameworkType), "filter[tender.techniques.frameworkAgreement.type]",
            frameworkType);

    public ICommercialToolsQueryBuilder WithBuyerClassificationRestrictions(string restrictionId) =>
        WithParameterIf(!string.IsNullOrWhiteSpace(restrictionId),
            "filter[tender.techniques.frameworkAgreement.buyerClassificationRestrictions.id]", restrictionId);

    public ICommercialToolsQueryBuilder ExcludeBuyerClassificationRestrictions(string restrictionId) =>
        WithParameterIf(!string.IsNullOrWhiteSpace(restrictionId),
            "filter[tender.techniques.frameworkAgreement.buyerClassificationRestrictions.id ne]", restrictionId);

    public ICommercialToolsQueryBuilder WithPageSize(int size) =>
        WithParameter("page[size]", size.ToString());

    public ICommercialToolsQueryBuilder WithPageNumber(int number) =>
        WithParameter("page[number]", number.ToString());

    public string Build(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl));

        var sb = new StringBuilder(baseUrl);
        var sep = baseUrl.Contains('?') ? '&' : '?';
        if (_params.Any())
            sb.Append(sep)
                .Append(string.Join('&', _params.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}")));

        return sb.ToString();
    }
}