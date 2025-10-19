namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

public class SearchRequestDto
{
    public List<string>? Keywords { get; set; }
    public KeywordSearchMode SearchMode { get; set; } = KeywordSearchMode.Any;
    public string? Status { get; set; }
    public DateTime? SubmissionDeadlineFrom { get; set; }
    public DateTime? SubmissionDeadlineTo { get; set; }
    public DateTime? ContractStartDateFrom { get; set; }
    public DateTime? ContractStartDateTo { get; set; }
    public DateTime? ContractEndDateFrom { get; set; }
    public DateTime? ContractEndDateTo { get; set; }
    public decimal? MinFees { get; set; }
    public decimal? MaxFees { get; set; }
    public List<string>? AwardMethod { get; set; }
    public string? FrameworkOptions { get; set; }
    public string? DynamicMarketOptions { get; set; }
    public List<string>? ContractingAuthorityUsage { get; set; }
    public List<string>? CpvCodes { get; set; }
    public List<string>? LocationCodes { get; set; }
    public string? SortBy { get; set; }
    public int PageNumber { get; set; } = 1;
    public int? Skip { get; set; }
    public int? Top { get; set; }
}