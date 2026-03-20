using CO.CDP.UserManagement.Core.Constants;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Thin anti-corruption layer between User Management and the Organisation API.
/// Keeps UM.Core and UM.Infrastructure free of generated WebApiClient types.
/// </summary>
public interface IOrganisationApiAdapter
{
    /// <summary>Returns the party roles for the given CDP organisation, or an empty set if not found.</summary>
    Task<ISet<PartyRole>> GetPartyRolesAsync(Guid cdpOrganisationGuid, CancellationToken cancellationToken = default);

    /// <summary>Creates a person invite via the Organisation API and returns the new invite's GUID.</summary>
    Task<Guid> CreatePersonInviteAsync(
        Guid cdpOrganisationGuid,
        string email,
        string firstName,
        string lastName,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default);
}
