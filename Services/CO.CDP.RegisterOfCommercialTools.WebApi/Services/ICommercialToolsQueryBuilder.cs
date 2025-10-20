using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public interface ICommercialToolsQueryBuilder
{
    ICommercialToolsQueryBuilder WithKeywords(List<string>? keywords, KeywordSearchMode searchMode);
    ICommercialToolsQueryBuilder WithFrameworkAgreement(bool hasFramework = true);
    ICommercialToolsQueryBuilder WithDynamicPurchasingSystem(bool hasDps = true);
    ICommercialToolsQueryBuilder OnlyOpenFrameworks(bool only = true);
    ICommercialToolsQueryBuilder WithStatus(string status);
    ICommercialToolsQueryBuilder FeeFrom(decimal from);
    ICommercialToolsQueryBuilder FeeTo(decimal to);
    ICommercialToolsQueryBuilder SubmissionDeadlineFrom(DateTime from);
    ICommercialToolsQueryBuilder SubmissionDeadlineTo(DateTime to);
    ICommercialToolsQueryBuilder ContractStartDateFrom(DateTime from);
    ICommercialToolsQueryBuilder ContractStartDateTo(DateTime to);
    ICommercialToolsQueryBuilder ContractEndDateFrom(DateTime from);
    ICommercialToolsQueryBuilder ContractEndDateTo(DateTime to);
    ICommercialToolsQueryBuilder WithFrameworkType(string frameworkType);
    ICommercialToolsQueryBuilder WithBuyerClassificationRestrictions(string restrictionId);
    ICommercialToolsQueryBuilder ExcludeBuyerClassificationRestrictions(string restrictionId);
    ICommercialToolsQueryBuilder WithLocation(string location);
    ICommercialToolsQueryBuilder WithCpv(string cpv);
    ICommercialToolsQueryBuilder WithAwardMethod(string awardMethod);
    ICommercialToolsQueryBuilder WithSkip(int skip);
    ICommercialToolsQueryBuilder WithTop(int top);
    ICommercialToolsQueryBuilder WithCustomFilter(string filter);
    ICommercialToolsQueryBuilder WithOrderBy(string sortBy);
    string Build(string baseUrl);
}