namespace CO.CDP.OrganisationInformation;

public record BuyerInformation
{

    public string? BuyerType { get; set; }

    public List<DevolvedRegulation> DevolvedRegulations { get; set; } = [];
    
}