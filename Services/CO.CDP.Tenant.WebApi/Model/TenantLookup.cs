using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Tenant.WebApi.Model;

public record TenantLookup
{
    public required UserDetails User { get; init; }
    public required List<UserTenant> Tenants { get; init; } = [];
}

public record UserDetails
{
    /// <example>"urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302"</example>
    public required string Urn { get; init; }

    /// <example>"John Doe"</example>
    public required string Name { get; init; }

    /// <example>"john@example.com"</example>
    [EmailAddress]
    public required string Email { get; init; }
}

public record UserTenant
{
    /// <example>"826def77-311c-424d-a86e-069029c859c0"</example>
    public required Guid Id { get; init; }

    /// <example>"Acme Corporation / John Doe"</example>
    public required string Name { get; init; }

    public required List<UserOrganisation> Organisations { get; init; } = [];
}

public record UserOrganisation
{
    /// <example>"f4596cdd-12e5-4f25-9db1-4312474e516f"</example>
    public required Guid Id { get; init; }

    /// <example>"Acme Group Ltd"</example>
    public required string Name { get; init; }

    public required List<PartyRole> Roles { get; init; }

    /// <example>"https://cdp.cabinetoffice.gov.uk/organisations/f4596cdd-12e5-4f25-9db1-4312474e516f"</example>
    public required Uri Uri { get; init; }

    /// <example>["Responder"]</example>
    public required List<string> Scopes { get; init; }
}