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
}