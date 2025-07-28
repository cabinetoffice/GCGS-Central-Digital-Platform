namespace CO.CDP.RegisterOfCommercialTools.WebApi.Models;

public class SearchRequestDto
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public DateTime? SubmissionDeadlineFrom { get; set; }
    public DateTime? SubmissionDeadlineTo { get; set; }
    public DateTime? ContractStartDateFrom { get; set; }
    public DateTime? ContractStartDateTo { get; set; }
    public DateTime? ContractEndDateFrom { get; set; }
    public DateTime? ContractEndDateTo { get; set; }
    public decimal? MinFees { get; set; }
    public decimal? MaxFees { get; set; }
    public string? AwardMethod { get; set; }
    public string? SortBy { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}