namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public record SearchResult(
    Guid Id,
    string Title,
    string Caption,
    string CommercialTool,
    SearchResultStatus Status,
    string MaximumFee,
    string OtherContractingAuthorityCanUse,
    string SubmissionDeadline,
    string ContractDates,
    string AwardMethod
);
