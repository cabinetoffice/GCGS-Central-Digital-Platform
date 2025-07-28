namespace CO.CDP.RegisterOfCommercialTools.WebApi.Models;

public class SearchResultDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Link { get; set; } = null!;
    public DateTime PublishedDate { get; set; }
    public DateTime? SubmissionDeadline { get; set; }
    public CommercialToolStatus Status { get; set; }
    public decimal Fees { get; set; }
    public string AwardMethod { get; set; } = null!;
    public string? ReservedParticipation { get; set; }
}