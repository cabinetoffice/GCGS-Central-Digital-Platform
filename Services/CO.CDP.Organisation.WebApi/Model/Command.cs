using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

public record RegisterOrganisation
{
    /// <example>"Acme Corporation"</example>
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

    /// <example>"d230dbc1-b273-4e0e-8f58-d94f2ab3c096"</example>
    [Required] public required Guid PersonId { get; init; }

    public required OrganisationIdentifier Identifier { get; init; }

    public List<OrganisationIdentifier>? AdditionalIdentifiers { get; init; }

    public List<OrganisationAddress> Addresses { get; init; } = [];

    public required OrganisationContactPoint? ContactPoint { get; init; }

    public required List<PartyRole> Roles { get; init; }

}

internal record UpdateOrganisation
{
    /// <example>"Acme Corporation"</example>
    [Required]
    public required string Name { get; init; }

    [Required]
    public required OrganisationIdentifier Identifier { get; init; }

    public List<OrganisationIdentifier>? AdditionalIdentifiers { get; init; }

    [Required]
    public List<OrganisationAddress> Addresses { get; init; } = [];

    public OrganisationContactPoint? ContactPoint { get; init; }

    public List<PartyRole>? Roles { get; init; }
}

public record OrganisationIdentifier
{
    /// <example>"CDP-PPON"</example>
    [Required(AllowEmptyStrings = false)]
    public required string Scheme { get; init; }

    /// <example>"5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public required string Id { get; init; }

    /// <example>"Acme Corporation Ltd."</example>
    public required string LegalName { get; init; }
}

public record OrganisationAddress
{
    [Required]
    public required AddressType Type { get; init; }

    /// <example>"82 St. John’s Road"</example>
    [Required(AllowEmptyStrings = false)]
    public required string StreetAddress { get; init; }

    /// <example>"Green Tower"</example>
    public string? StreetAddress2 { get; init; }

    /// <example>"CHESTER"</example>
    [Required(AllowEmptyStrings = false)]
    public required string Locality { get; init; }

    /// <example>"Lancashire"</example>
    public string? Region { get; init; }

    /// <example>"CH43 7UR"</example>
    public required string PostalCode { get; init; }

    /// <example>"United Kingdom"</example>
    public required string CountryName { get; init; }
}

public record OrganisationContactPoint
{
    /// <example>"Procurement Team"</example>
    public string? Name { get; init; }

    /// <example>"procurement@example.com"</example>
    [Required(AllowEmptyStrings = false)]
    public required string Email { get; init; }

    /// <example>"079256123321"</example>
    public string? Telephone { get; init; }

    /// <example>"https://example.com"</example>
    public string? Url { get; init; }
}

public static class MappingExtensions
{
    public static Identifier AsView(this OrganisationIdentifier command) =>
        new()
        {
            Scheme = command.Scheme,
            Id = command.Id,
            LegalName = command.LegalName,
            Uri = null
        };

    public static List<Identifier> AsView(this List<OrganisationIdentifier>? command) =>
        command?.Select(i => i.AsView()).ToList() ?? [];

    public static Address AsView(this OrganisationAddress command) =>
        new()
        {
            Type = command.Type,
            StreetAddress = command.StreetAddress,
            StreetAddress2 = command.StreetAddress2,
            Locality = command.Locality,
            Region = command.Region,
            PostalCode = command.PostalCode,
            CountryName = command.CountryName
        };

    public static List<Address> AsView(this List<OrganisationAddress> command) =>
        command.Select(i => i.AsView()).ToList() ?? [];

    public static ContactPoint AsView(this OrganisationContactPoint? command) =>
        command != null ? new()
        {
            Name = command.Name ?? "",
            Email = command.Email,
            Telephone = command.Telephone,
            Url = command.Url != null ? new Uri(command.Url) : null
        } : new ContactPoint();
}
