using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

public record OrganisationSearchByPponResult
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required OrganisationType Type { get; init; }

    public required Identifier Identifier { get; init; }

    public required List<PartyRole> Roles { get; init; }

    public required List<OrganisationAddress> Addresses { get; init; }
}