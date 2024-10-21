using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;
public record OrganisationExtended
{
    /// <example>"d2dab085-ec23-481c-b970-ee6b372f9f57"</example>
    public required Guid Id { get; init; }

    /// <example>"Acme Corporation"</example>
    public required string Name { get; init; }

    public required Identifier Identifier { get; init; }

    public List<Identifier> AdditionalIdentifiers { get; init; } = [];

    public List<Address> Addresses { get; init; } = [];

    public required ContactPoint ContactPoint { get; init; }

    /// <example>["supplier"]</example>
    public required List<PartyRole> Roles { get; init; }

    public required Details Details { get; init; }
}
