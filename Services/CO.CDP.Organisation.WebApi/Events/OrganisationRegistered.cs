namespace CO.CDP.Organisation.WebApi.Events;

public record OrganisationRegistered
{
    /// <example>"d2dab085-ec23-481c-b970-ee6b372f9f57"</example>
    public required string Id { get; init; }

    /// <example>"Acme Corporation"</example>
    public required string Name { get; init; }

    public required Identifier Identifier { get; init; }

    public List<Identifier> AdditionalIdentifiers { get; init; } = [];

    public List<Address> Addresses { get; init; } = [];

    public required ContactPoint ContactPoint { get; init; }

    public required List<string> Roles { get; init; }
}