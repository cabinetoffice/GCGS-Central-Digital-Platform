using CO.CDP.UserManagement.Core.Constants;
using CO.CDP.UserManagement.Core.Models;

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

    /// <summary>Resends an existing person invite, extending its lifespan and re-sending the notification email.</summary>
    Task ResendPersonInviteAsync(
        Guid cdpOrganisationGuid,
        Guid personInviteGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all persons in the given organisation with their Organisation Information scopes.
    /// Returns an empty list if the organisation is not found.
    /// </summary>
    Task<IReadOnlyList<OiOrganisationPerson>> GetOrganisationPersonsAsync(
        Guid cdpOrganisationGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all pending person invites for the given organisation.
    /// Returns an empty list if the organisation is not found.
    /// </summary>
    Task<IReadOnlyList<OiPersonInvite>> GetOrganisationPersonInvitesAsync(
        Guid cdpOrganisationGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all join requests for the given organisation.
    /// Returns an empty list if the organisation is not found.
    /// </summary>
    Task<IReadOnlyList<OiJoinRequest>> GetOrganisationJoinRequestsAsync(
        Guid cdpOrganisationGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves or rejects a join request via the Organisation API.
    /// The Organisation API handles OI-side mutations and sends GOV.UK Notify emails.
    /// </summary>
    Task UpdateJoinRequestAsync(
        Guid cdpOrganisationGuid,
        Guid joinRequestId,
        string status,
        Guid reviewedByPersonId,
        CancellationToken cancellationToken = default);
}