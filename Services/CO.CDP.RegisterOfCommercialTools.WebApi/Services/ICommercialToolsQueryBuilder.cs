using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public interface ICommercialToolsQueryBuilder
{
    bool HasFilter { get; }
    ICommercialToolsQueryBuilder WithKeywords(List<string>? keywords, KeywordSearchMode searchMode);
    ICommercialToolsQueryBuilder WithFrameworkAgreement(bool hasFramework = true);
    ICommercialToolsQueryBuilder WithDynamicPurchasingSystem(bool hasDps = true);
    ICommercialToolsQueryBuilder OnlyOpenFrameworks(bool only = true);
    ICommercialToolsQueryBuilder WithStatuses(List<string>? statuses);
    ICommercialToolsQueryBuilder FeeFrom(decimal from);
    ICommercialToolsQueryBuilder FeeTo(decimal to);
    ICommercialToolsQueryBuilder SubmissionDeadlineFrom(DateTime from);
    ICommercialToolsQueryBuilder SubmissionDeadlineTo(DateTime to);
    ICommercialToolsQueryBuilder ContractStartDate(DateTime from);
    ICommercialToolsQueryBuilder ContractEndDate(DateTime to);
    ICommercialToolsQueryBuilder WithFrameworkType(string frameworkType);
    ICommercialToolsQueryBuilder WithBuyerClassificationRestrictions(string restrictionId);
    ICommercialToolsQueryBuilder WithLocationCodes(List<string>? locationCodes);
    ICommercialToolsQueryBuilder WithCpvCodes(List<string>? cpvCodes);
    ICommercialToolsQueryBuilder WithAwardMethods(List<string>? awardMethods);
    ICommercialToolsQueryBuilder WithSkip(int skip);
    ICommercialToolsQueryBuilder WithTop(int top);
    ICommercialToolsQueryBuilder WithCustomFilter(string filter);
    ICommercialToolsQueryBuilder WithOrderBy(string sortBy);
    string Build(string baseUrl);
}