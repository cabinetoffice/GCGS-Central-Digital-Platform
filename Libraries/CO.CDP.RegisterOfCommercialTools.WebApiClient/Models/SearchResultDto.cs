using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

public class SearchResultDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public DateTime? PublishedDate { get; set; }
    public DateTime? SubmissionDeadline { get; set; }
    public CommercialToolStatus? Status { get; set; }
    public decimal? Fees { get; set; }
    public string? AwardMethod { get; set; }
    public string? OtherContractingAuthorityCanUse { get; set; }
    public string? ContractDates { get; set; }
    public string? CommercialTool { get; set; }
    public TechniquesInfo? Techniques { get; set; }
    public Dictionary<string, string>? AdditionalProperties { get; set; }
}