using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

public class SearchResultDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Url { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string SubmissionDeadline { get; set; } = string.Empty;
    public CommercialToolStatus Status { get; set; }
    public string MaximumFee { get; set; } = string.Empty;
    public string AwardMethod { get; set; } = string.Empty;
    public string OtherContractingAuthorityCanUse { get; set; } = string.Empty;
    public string ContractDates { get; set; } = string.Empty;
    public string CommercialTool { get; set; } = string.Empty;
    public TechniquesInfo? Techniques { get; set; }
    public Dictionary<string, string>? AdditionalProperties { get; set; }
}