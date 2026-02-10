using CO.CDP.Organisation.WebApiClient;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.Api.Models;

/// <summary>
/// Extension methods for pending invite mapping.
/// </summary>
public static class PendingOrganisationInviteMappingExtensions
{
    /// <summary>
    /// Converts a pending invite entity to a response model.
    /// </summary>
    public static PendingOrganisationInviteResponse ToResponse(
        this PendingOrganisationInvite invite,
        PersonInviteModel? personInvite)
    {
        return new PendingOrganisationInviteResponse
        {
            PendingInviteId = invite.Id,
            OrganisationId = invite.OrganisationId,
            CdpPersonInviteGuid = invite.CdpPersonInviteGuid,
            Email = personInvite?.Email ?? invite.Email,
            FirstName = personInvite?.FirstName,
            LastName = personInvite?.LastName,
            OrganisationRole = invite.OrganisationRole,
            Status = UserStatus.Pending,
            InvitedBy = invite.InvitedBy,
            ExpiresOn = personInvite?.ExpiresOn,
            CreatedAt = invite.CreatedAt
        };
    }
}
