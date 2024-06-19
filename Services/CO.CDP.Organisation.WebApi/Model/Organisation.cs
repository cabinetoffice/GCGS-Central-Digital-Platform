using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/reference/#parties">Party</a>.
/// </summary>
public record Organisation
{
    /// <example>"d2dab085-ec23-481c-b970-ee6b372f9f57"</example>
    public required Guid Id { get; init; }

    /// <example>"Acme Corporation"</example>
    public required string Name { get; init; }

    public required Identifier Identifier { get; init; }

    public List<Identifier> AdditionalIdentifiers { get; init; } = [];

    public List<Address> Addresses { get; init; } = [];

    public required ContactPoint ContactPoint { get; init; }

    /// <example>["Supplier"]</example>
    public required List<PartyRole> Roles { get; init; }
}

public record SupplierInformation
{
    public required string OrganisationName { get; set; }
    public SupplierType? SupplierType { get; set; }
    public List<OperationType> OperationTypes { get; set; } = [];
    public bool CompletedRegAddress { get; set; }
    public bool CompletedPostalAddress { get; set; }
    public bool CompletedVat { get; set; }
    public bool CompletedWebsiteAddress { get; set; }
    public bool CompletedEmailAddress { get; set; }
    public bool CompletedQualification { get; set; }
    public bool CompletedTradeAssurance { get; set; }
    public bool CompletedOperationType { get; set; }
    public bool CompletedLegalForm { get; set; }
    public List<TradeAssurance> TradeAssurances { get; set; } = [];
    public LegalForm? LegalForm { get; set; }
}

public record TradeAssurance
{
    public Guid? Id { get; set; }
    public required string AwardedByPersonOrBodyName { get; set; }
    public required string ReferenceNumber { get; set; }
    public required DateTimeOffset DateAwarded { get; set; }
}

public record LegalForm
{
    public required string RegisteredUnderAct2006 { get; set; }
    public required string RegisteredLegalForm { get; set; }
    public required string LawRegistered { get; set; }
    public required DateTimeOffset RegistrationDate { get; set; }
}