namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.TenderInfo;

public class FrameworkAgreement
{
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? Method { get; set; }
    public bool? IsOpenFrameworkScheme { get; set; }
    public FrameworkAgreementPeriod? Period { get; set; }
    public List<BuyerClassificationRestriction>? BuyerClassificationRestrictions { get; set; }

    public string? PeriodRationale { get; set; }
    public string? BuyerCategories { get; set; }
    public int? MaximumParticipants { get; set; }
    public decimal? ValueAmount { get; set; }
    public string? ValueCurrency { get; set; }
    public DateTime? OpenFrameworkSchemeEndDate { get; set; }
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
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