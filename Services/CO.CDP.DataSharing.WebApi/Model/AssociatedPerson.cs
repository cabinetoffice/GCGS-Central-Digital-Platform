using CO.CDP.OrganisationInformation;

namespace CO.CDP.DataSharing.WebApi.Model;

public record AssociatedPerson
{
    /// <example>"c16f9f7b-3f10-42db-86f8-93607b034a4c"</example>
    public required Guid Id { get; init; }
    public ConnectedPersonType EntityType { get; init; }
    public required AssociatedRelationship Relationship { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public DateTimeOffset? DateOfBirth { get; init; }
    public string? Nationality { get; init; }
    public string? ResidentCountry { get; init; }
    public IEnumerable<ControlCondition> ControlCondition { get; init; } = [];
    public IEnumerable<Address> Addresses { get; init; } = [];
    public string? RegistrationAuthority { get; init; }
    public DateTimeOffset? RegisteredDate { get; init; }
    public bool HasCompanyHouseNumber { get; init; }
    public string? CompanyHouseNumber { get; init; }
    public string? OverseasCompanyNumber { get; init; }
    public required AssociatedPeriod Period { get; init; }
}

public record AssociatedEntity
{
    /// <example>"f4596cdd-12e5-4f25-9db1-4312474e516f"</example>
    public required Guid Id { get; init; }
    public string EntityType { get; private init; } = "Organisation";
    public required string Name { get; init; }
    public required ConnectedOrganisationCategory Category { get; init; }
    public DateTimeOffset? InsolvencyDate { get; init; }
    public string? RegisteredLegalForm { get; init; }
    public string? LawRegistered { get; init; }
    public IEnumerable<ControlCondition> ControlCondition { get; init; } = [];
    public IEnumerable<Address> Addresses { get; init; } = [];
    public string? RegistrationAuthority { get; init; }
    public DateTimeOffset? RegisteredDate { get; init; }
    public bool HasCompanyHouseNumber { get; init; }
    public string? CompanyHouseNumber { get; init; }
    public string? OverseasCompanyNumber { get; init; }
    public required AssociatedPeriod Period { get; init; }
}

public enum AssociatedRelationship
{
    PersonWithSignificantControlForIndividual = 1,
    DirectorOrIndividualWithTheSameResponsibilitiesForIndividual,
    AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual,
    PersonWithSignificantControlForTrust,
    DirectorOrIndividualWithTheSameResponsibilitiesForTrust,
    AnyOtherIndividualWithSignificantInfluenceOrControlForTrust
}

public record AssociatedPeriod
{
    public DateTimeOffset? EndDate { get; init; }
}