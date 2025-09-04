namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

public class Party
{
    public PartyIdentifier? Identifier { get; set; }
    public List<AdditionalIdentifier>? AdditionalIdentifiers { get; set; }
}

public class PartyIdentifier
{
    public string? Id { get; set; }
}

public class AdditionalIdentifier
{
    public string? Id { get; set; }
}