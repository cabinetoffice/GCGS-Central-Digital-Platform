using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Organisation.WebApi.Model;

public record RegisterOrganisation
{
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

internal record UpdateOrganisation
{
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

    [Required(AllowEmptyStrings = true)]
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