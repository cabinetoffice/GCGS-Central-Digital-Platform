using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public record SearchResult(
    string Id,
    string Title,
    string Caption,
    string CommercialTool,
    CommercialToolStatus Status,
    string MaximumFee,
    string OtherContractingAuthorityCanUse,
    string SubmissionDeadline,
    string ContractDates,
    string AwardMethod,
    string? Url
);
