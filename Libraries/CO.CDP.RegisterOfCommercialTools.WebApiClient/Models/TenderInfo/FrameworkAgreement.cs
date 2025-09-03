namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

public class FrameworkAgreement
{
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? Method { get; set; }
    public bool? IsOpenFrameworkScheme { get; set; }
    public FrameworkAgreementPeriod? Period { get; set; }
    public List<BuyerClassificationRestriction>? BuyerClassificationRestrictions { get; set; }
}

public class FrameworkAgreementPeriod
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class BuyerClassificationRestriction
{
    public string? Id { get; set; }
    public string? Description { get; set; }
}