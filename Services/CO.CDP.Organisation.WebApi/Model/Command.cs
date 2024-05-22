using System.ComponentModel.DataAnnotations;

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

public class DeleteOrganisationRequest
{
    public Guid OrganisationId { get; set; }
}