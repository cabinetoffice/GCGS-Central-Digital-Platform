using CO.CDP.OrganisationInformation;
using System.Text.Json.Serialization;

namespace CO.CDP.Organisation.WebApi.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PartyRoleStatus
{
    Active,
    Pending
}

public record PartyRoleWithStatus
{
    public required PartyRole Role { get; init; }
    public required PartyRoleStatus Status { get; init; }
}

public record OrganisationSearchByPponResult
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required OrganisationType Type { get; init; }

    public required List<Identifier> Identifiers { get; init; }

    public required List<PartyRoleWithStatus> PartyRoles { get; init; }

    public required List<OrganisationAddress> Addresses { get; init; }
}