namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

public class TechniquesInfo
{
    public bool? HasFrameworkAgreement { get; set; }
    public bool? HasDynamicPurchasingSystem { get; set; }
    public bool? HasElectronicAuction { get; set; }
    public FrameworkAgreement? FrameworkAgreement { get; set; }
}