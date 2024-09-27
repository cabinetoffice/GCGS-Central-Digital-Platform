using CO.CDP.OrganisationInformation;
using System.Text.Json.Serialization;

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

    /// <example>["supplier"]</example>
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
    public bool CompletedOperationType { get; set; }
    public bool CompletedLegalForm { get; set; }
    public bool CompletedConnectedPerson { get; set; }
    public LegalForm? LegalForm { get; set; }
}

public record LegalForm
{
    public required bool RegisteredUnderAct2006 { get; set; }
    public required string RegisteredLegalForm { get; set; }
    public required string LawRegistered { get; set; }
    public required DateTimeOffset RegistrationDate { get; set; }
}

public record ConnectedEntity
{
    public Guid Id { get; set; }
    public required ConnectedEntityType EntityType { get; set; }
    public bool HasCompnayHouseNumber { get; set; }
    public string? CompanyHouseNumber { get; set; }
    public string? OverseasCompanyNumber { get; set; }

    public ConnectedOrganisation? Organisation { get; set; }
    public ConnectedIndividualTrust? IndividualOrTrust { get; set; }
    public ICollection<Address> Addresses { get; set; } = [];

    public DateTimeOffset? RegisteredDate { get; set; }
    public string? RegisterName { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

public record ConnectedIndividualTrust
{
    public int Id { get; set; }
    public required ConnectedIndividualAndTrustCategory Category { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public ICollection<ControlCondition> ControlCondition { get; set; } = [];
    public ConnectedPersonType ConnectedType { get; set; }
    public Guid? PersonId { get; set; }
    public string? ResidentCountry { get; set; }
}

public record ConnectedOrganisation
{
    public int Id { get; set; }
    public required ConnectedOrganisationCategory Category { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset? InsolvencyDate { get; set; }
    public string? RegisteredLegalForm { get; set; }
    public string? LawRegistered { get; set; }
    public ICollection<ControlCondition> ControlCondition { get; set; } = [];
    public Guid? OrganisationId { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedEntityType
{
    Organisation = 1,
    Individual,
    TrustOrTrustee
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ControlCondition
{
    None,
    OwnsShares,
    HasVotingRights,
    CanAppointOrRemoveDirectors,
    HasOtherSignificantInfluenceOrControl
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedPersonType
{
    Individual = 1,
    TrustOrTrustee
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedIndividualAndTrustCategory
{
    PersonWithSignificantControlForIndividual = 1,
    DirectorOrIndividualWithTheSameResponsibilitiesForIndividual,
    AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual,
    PersonWithSignificantControlForTrust,
    DirectorOrIndividualWithTheSameResponsibilitiesForTrust,
    AnyOtherIndividualWithSignificantInfluenceOrControlForTrust
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedOrganisationCategory
{
    RegisteredCompany = 1,
    DirectorOrTheSameResponsibilities,
    ParentOrSubsidiaryCompany,
    ACompanyYourOrganisationHasTakenOver,
    AnyOtherOrganisationWithSignificantInfluenceOrControl,
}