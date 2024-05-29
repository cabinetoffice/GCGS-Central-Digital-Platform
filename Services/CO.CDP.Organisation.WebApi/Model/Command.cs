using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

public record RegisterOrganisation
{
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    [Required] public required Guid PersonId { get; init; }

    public required OrganisationIdentifier Identifier { get; init; }

    public List<OrganisationIdentifier>? AdditionalIdentifiers { get; init; }

    [Required]
    public required OrganisationAddress Address { get; init; }

    public required OrganisationContactPoint? ContactPoint { get; init; }

    public List<int>? Types { get; init; }

}

internal record UpdateOrganisation
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required OrganisationIdentifier Identifier { get; init; }

    public List<OrganisationIdentifier>? AdditionalIdentifiers { get; init; }

    [Required]
    public required OrganisationAddress Address { get; init; }

    public OrganisationContactPoint? ContactPoint { get; init; }

    public List<int>? Types { get; init; }
}

public record OrganisationIdentifier
{
    [Required(AllowEmptyStrings = false)]
    public required string Scheme { get; init; }
    public required string Id { get; init; }
    public required string LegalName { get; init; }
}

public record OrganisationAddress
{
    [Required(AllowEmptyStrings = false)]
    public required string StreetAddress { get; init; }

    public string? StreetAddress2 { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Locality { get; init; }

    public string? Region { get; init; }

    public required string PostalCode { get; init; }

    public required string CountryName { get; init; }
}

public record OrganisationContactPoint
{
    public string? Name { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Email { get; init; }
    public string? Telephone { get; init; }
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
            StreetAddress = command.StreetAddress,
            StreetAddress2 = command.StreetAddress2,
            Locality = command.Locality,
            Region = command.Region,
            PostalCode = command.PostalCode,
            CountryName = command.CountryName
        };

    public static ContactPoint AsView(this OrganisationContactPoint command) =>
        new()
        {
            Name = command.Name ?? "",
            Email = command.Email,
            Telephone = command.Telephone,
            Url = command.Url != null ? new Uri(command.Url) : null
        };
}