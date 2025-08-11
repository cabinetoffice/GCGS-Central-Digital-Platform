namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public interface ICommercialToolsQueryBuilder
{
    ICommercialToolsQueryBuilder WithKeywords(string keywords);
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
    ICommercialToolsQueryBuilder ContractLocation(string region);
    ICommercialToolsQueryBuilder WithCpv(string cpv);
    ICommercialToolsQueryBuilder WithPageSize(int size);
    ICommercialToolsQueryBuilder WithPageNumber(int number);
    string Build(string baseUrl);
}