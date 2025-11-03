namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

public class SearchRequestDto
{
    public List<string>? Keywords { get; set; }
    public KeywordSearchMode SearchMode { get; set; } = KeywordSearchMode.Any;
    public string? Status { get; set; }
    public DateTime? SubmissionDeadlineFrom { get; set; }
    public DateTime? SubmissionDeadlineTo { get; set; }
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public decimal? MinFees { get; set; }
    public decimal? MaxFees { get; set; }
    public List<string>? AwardMethod { get; set; }
    public bool FilterFrameworks { get; set; }
    public bool IsOpenFrameworks { get; set; }
    public bool FilterDynamicMarkets { get; set; }
    public bool IsUtilitiesOnly { get; set; }
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