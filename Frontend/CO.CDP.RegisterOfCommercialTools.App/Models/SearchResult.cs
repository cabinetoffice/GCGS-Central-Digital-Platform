namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public record SearchResult(
    string Id,
    string Title,
    string Caption,
    string CommercialTool,
    SearchResultStatus Status,
    string MaximumFee,
    string OtherContractingAuthorityCanUse, // In the future, this should be a Yes/No field derived from ReservedParticipation
    string SubmissionDeadline,
    string ContractDates,
    string AwardMethod,
    string? Url
);
