using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;

public class CommercialToolsApiResponse
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public List<CommercialToolApiItem>? Data { get; set; }
    public Paging? Paging { get; set; }
}

public class CommercialToolApiItem
{
    public string? TenderId { get; set; }
    public string? TenderIdentifier { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public CreatedAt? CreatedAt { get; set; }
    public TenderPeriod? TenderPeriod { get; set; }
    public TechniquesInfo? Techniques { get; set; }
}