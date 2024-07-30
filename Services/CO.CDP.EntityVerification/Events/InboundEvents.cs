namespace CO.CDP.EntityVerification.Events;

public interface IEvent;

public record OrganisationRegistered : IEvent
{
    /// <example>"d2dab085-ec23-481c-b970-ee6b372f9f57"</example>
    public required Guid Id { get; init; }

    /// <example>"Acme Corporation"</example>
    public required string Name { get; init; }

    public required Identifier Identifier { get; init; }

    public List<Identifier> AdditionalIdentifiers { get; init; } = [];

    public IEnumerable<Identifier> AllIdentifiers() => AdditionalIdentifiers.Prepend(Identifier);

    public required List<string> Roles { get; init; }
}

public record OrganisationUpdated : IEvent
{
    /// <example>"d2dab085-ec23-481c-b970-ee6b372f9f57"</example>
    public required Guid Id { get; init; }

    /// <example>"Acme Corporation"</example>
    public required string Name { get; init; }

    public required Identifier Identifier { get; init; }

    public List<Identifier> AdditionalIdentifiers { get; init; } = [];

    public IEnumerable<Identifier> AllIdentifiers() => Identifier != null ? AdditionalIdentifiers.Prepend(Identifier) : AdditionalIdentifiers;

    public required List<string> Roles { get; init; }

    public Identifier? FindIdentifierByScheme(string scheme)
    {
        return AllIdentifiers()
            .FirstOrDefault(i => i.Scheme == scheme);
    }
}