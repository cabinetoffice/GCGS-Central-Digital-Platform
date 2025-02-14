using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationInformation;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/reference/#organizationreference">OrganizationReference</a>.
/// </summary>
public record OrganisationReference
{
    /// <example>"f4596cdd-12e5-4f25-9db1-4312474e516f"</example>
    [Required] public required Guid Id { get; init; }
    /// <example>"Acme Group Ltd"</example>
    [Required] public required string Name { get; init; }
    /// <example>["supplier"]</example>
    [Required] public required List<PartyRole> Roles { get; init; }
    /// <example>"https://cdp.cabinetoffice.gov.uk/organisations/f4596cdd-12e5-4f25-9db1-4312474e516f"</example>
    public Uri? Uri { get; init; }
    public OrganisationReferenceShareCode? ShareCode { get; set; }

    public OrganisationReferenceDetails? Details { get; set; }
}

public record OrganisationReferenceShareCode
{
    /// <example>"QTnhQbHM"</example>
    public required string Value { get; set; }

    /// <example>"2025-01-30T17:41:37.109Z"</example>
    public required DateTimeOffset SubmittedAt { get; set; }
}

public record OrganisationReferenceDetails
{
    public required ConnectedOrganisationCategory Category { get; set; }
    public DateTimeOffset? InsolvencyDate { get; set; }
    public string? RegisteredLegalForm { get; set; }
    public string? LawRegistered { get; set; }
    public IEnumerable<ControlCondition> ControlCondition { get; set; } = [];

    public IEnumerable<Address> Addresses { get; set; } = [];
    public DateTimeOffset? RegisteredDate { get; set; }
    public string? RegistrationAuthority { get; set; } // RegisterName
    public bool HasCompanyHouseNumber { get; set; }
    public string? CompanyHouseNumber { get; set; }
    public string? OverseasCompanyNumber { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}