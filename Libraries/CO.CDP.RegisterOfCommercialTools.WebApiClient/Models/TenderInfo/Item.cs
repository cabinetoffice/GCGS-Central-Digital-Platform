namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

public class Item
{
    public List<AdditionalClassification>? AdditionalClassifications { get; set; }
    public List<DeliveryAddress>? DeliveryAddresses { get; set; }
}

public class AdditionalClassification
{
    public string? Scheme { get; set; }
    public string? Id { get; set; }
}

public class DeliveryAddress
{
    public string? Region { get; set; }
    public string? CountryName { get; set; }
    public string? CountryCode { get; set; }
}