namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

public class TenderInfoDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public Dictionary<string, string>? AdditionalProperties { get; set; }
    public List<ParticipationFee>? ParticipationFees { get; set; }
    public List<Party>? Parties { get; set; }
    public List<Item>? Items { get; set; }
    public TechniquesInfo? Techniques { get; set; }
}