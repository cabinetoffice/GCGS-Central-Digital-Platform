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
    public string? Id { get; set; }
    public string? Ocid { get; set; }
    public DateTime? Date { get; set; }
    public CommercialToolBuyer? Buyer { get; set; }
    public List<CommercialToolParty>? Parties { get; set; }
    public CommercialToolTender? Tender { get; set; }
    public List<CommercialToolAward>? Awards { get; set; }
    public List<CommercialToolContract>? Contracts { get; set; }
    public CreatedAt? CreatedAt { get; set; }
}

public class CommercialToolBuyer
{
    public string? Name { get; set; }
    public CommercialToolIdentifier? Identifier { get; set; }
}

public class CommercialToolParty
{
    public List<string>? Roles { get; set; }
    public string? Name { get; set; }
    public List<CommercialToolLocation>? Locations { get; set; }
}

public class CommercialToolLocation
{
    public CommercialToolPhysicalAddress? PhysicalAddress { get; set; }
}

public class CommercialToolPhysicalAddress
{
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? Locality { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
}

public class CommercialToolTender
{
    public string? TenderId { get; set; }
    public string? TenderIdentifier { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string? ProcurementMethod { get; set; }
    public string? ProcurementMethodDetails { get; set; }
    public CommercialToolTenderPeriod? TenderPeriod { get; set; }
    public CommercialToolTenderPeriod? ContractPeriod { get; set; }
    public CommercialToolTechniques? Techniques { get; set; }
    public CommercialToolValue? Value { get; set; }
    public List<CommercialToolLot>? Lots { get; set; }
    public List<CommercialToolParticipationFee>? ParticipationFees { get; set; }
    public CommercialToolAwardPeriod? AwardPeriod { get; set; }
}

public class CommercialToolParticipationFee
{
    public decimal? RelativeValueProportion { get; set; }
}

public class CommercialToolRelativeValue
{
    public decimal? Proportion { get; set; }
}

public class CommercialToolTenderPeriod
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CommercialToolTechniques
{
    public bool? HasFrameworkAgreement { get; set; }
    public bool? HasDynamicPurchasingSystem { get; set; }
    public bool? HasElectronicAuction { get; set; }
    public CommercialToolFrameworkAgreement? FrameworkAgreement { get; set; }
}

public class CommercialToolFrameworkAgreement
{
    public string? Method { get; set; }
    public string? Type { get; set; }
    public bool? IsOpenFrameworkScheme { get; set; }
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
    public CommercialToolFrameworkPeriod? Period { get; set; }
}

public class CommercialToolFrameworkPeriod
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CommercialToolValue
{
    public decimal? Amount { get; set; }
    public decimal? AmountGross { get; set; }
    public string? Currency { get; set; }
}

public class CommercialToolLot
{
    public string? LotIdentifier { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public CommercialToolTenderPeriod? ContractPeriod { get; set; }
}

public class CommercialToolIdentifier
{
    public string? Scheme { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Uri { get; set; }
}

public class CommercialToolAward
{
    public CommercialToolTenderPeriod? ContractPeriod { get; set; }
    public CommercialToolStandstillPeriod? StandstillPeriod { get; set; }
}

public class CommercialToolStandstillPeriod
{
    public DateTime? EndDate { get; set; }
}

public class CommercialToolAwardPeriod
{
    public DateTime? EndDate { get; set; }
}

public class CommercialToolContract
{
    public CommercialToolContractPeriod? Period { get; set; }
}

public class CommercialToolContractPeriod
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}