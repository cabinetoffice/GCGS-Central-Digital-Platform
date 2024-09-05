using CO.CDP.OrganisationInformation;

namespace CO.CDP.DataSharing.WebApi.Model;

public record BasicInformation
{
    public SupplierType? SupplierType { get; init; }
    public Address? RegisteredAddress { get; init; }
    public Address? PostalAddress { get; init; }
    public string? VatNumber { get; init; }
    public string? WebsiteAddress { get; init; }
    public string? EmailAddress { get; init; }
    public List<BasicQualification> Qualifications { get; init; } = new();
    public List<BasicTradeAssurance> TradeAssurances { get; init; } = new();
    public OrganisationType OrganisationType { get; init; }
    public BasicLegalForm? LegalForm { get; init; }
}

public record BasicQualification
{
    public Guid Guid { get; init; }
    public string AwardedByPersonOrBodyName { get; init; } = string.Empty;
    public DateTimeOffset DateAwarded { get; init; }
    public string Name { get; init; } = string.Empty;
}

public record BasicTradeAssurance
{
    public Guid Guid { get; init; }
    public string AwardedByPersonOrBodyName { get; init; } = string.Empty;
    public string ReferenceNumber { get; init; } = string.Empty;
    public DateTimeOffset DateAwarded { get; init; }
}

public record BasicLegalForm
{
    public bool RegisteredUnderAct2006 { get; init; }
    public string RegisteredLegalForm { get; init; } = string.Empty;
    public string LawRegistered { get; init; } = string.Empty;
    public DateTimeOffset RegistrationDate { get; init; }
}

public enum OrganisationType
{
    Buyer = 1,
    Supplier = 2
}
