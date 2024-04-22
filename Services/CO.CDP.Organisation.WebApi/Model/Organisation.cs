using System.ComponentModel.DataAnnotations;
namespace CO.CDP.Organisation.WebApi.Model;

public record Organisation
{
    [Required(AllowEmptyStrings = true)] public required Guid Id { get; init; }

    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

    [Required]
    public required OrganisationIdentifier Identifier { get; init; }

    [Required]
    public required List<OrganisationIdentifier> AdditionalIdentifiers { get; init; }

    [Required]
    public required OrganisationAddress Address { get; init; }

    [Required]
    public required OrganisationContactPoint ContactPoint { get; init; }

    [Required]
    public required List<int> Roles { get; init; }
}

public record OrganisationIdentifier
{
    [Required(AllowEmptyStrings = false)]
    public required string Scheme { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Id { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string LegalName { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Uri { get; init; }
}

public record OrganisationAddress
{
    [Required(AllowEmptyStrings = false)]
    public required string StreetAddress { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Locality { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Region { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string PostalCode { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string CountryName { get; init; }
}

public record OrganisationContactPoint
{
    [Required(AllowEmptyStrings = false)]
    public required string Name { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Email { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Telephone { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string FaxNumber { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Url { get; init; }
}
