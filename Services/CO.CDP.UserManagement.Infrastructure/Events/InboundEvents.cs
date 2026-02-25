using CO.CDP.OrganisationInformation;

namespace CO.CDP.UserManagement.Infrastructure.Events;

public interface IEvent;

/// <summary>
/// Event published when a new organisation is registered in CDP.
/// </summary>
public record OrganisationRegistered : IEvent
{
    /// <summary>
    /// CDP organisation GUID.
    /// </summary>
    /// <example>"d2dab085-ec23-481c-b970-ee6b372f9f57"</example>
    public required string Id { get; init; }

    /// <summary>
    /// Organisation name.
    /// </summary>
    /// <example>"Acme Corporation"</example>
    public required string Name { get; init; }

    public required Identifier Identifier { get; init; }

    public List<Identifier> AdditionalIdentifiers { get; init; } = [];

    public List<Address> Addresses { get; init; } = [];

    public required ContactPoint ContactPoint { get; init; }

    public required List<string> Roles { get; init; }

    public required OrganisationType Type { get; init; }
}

/// <summary>
/// Event published when an organisation is updated in CDP.
/// </summary>
public record OrganisationUpdated : IEvent
{
    /// <summary>
    /// CDP organisation GUID.
    /// </summary>
    /// <example>"d2dab085-ec23-481c-b970-ee6b372f9f57"</example>
    public required string Id { get; init; }

    /// <summary>
    /// Organisation name.
    /// </summary>
    /// <example>"Acme Corporation"</example>
    public required string Name { get; init; }

    public required Identifier Identifier { get; init; }

    public List<Identifier> AdditionalIdentifiers { get; init; } = [];

    public List<Address> Addresses { get; init; } = [];

    public required ContactPoint ContactPoint { get; init; }

    public required List<string> Roles { get; init; }
}

/// <summary>
/// Event published when a user claims a PersonInvite in CDP.
/// </summary>
public record PersonInviteClaimed : IEvent
{
    /// <summary>
    /// CDP PersonInvite GUID.
    /// </summary>
    public required Guid PersonInviteGuid { get; init; }

    /// <summary>
    /// CDP Person GUID.
    /// </summary>
    public required Guid PersonGuid { get; init; }

    /// <summary>
    /// User URN (OneLogin sub claim).
    /// </summary>
    public required string UserUrn { get; init; }

    /// <summary>
    /// CDP Organisation GUID.
    /// </summary>
    public required Guid OrganisationGuid { get; init; }
}
