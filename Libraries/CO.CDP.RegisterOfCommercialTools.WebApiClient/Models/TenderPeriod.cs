namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

public class TenderPeriod
{
    public string? TenderId { get; set; }
    public string? PeriodType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? MaxExtentDate { get; set; }
    public int? DurationInDays { get; set; }
    public int? DurationInMonths { get; set; }
}