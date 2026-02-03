using CO.CDP.ApplicationRegistry.Core.Entities;
using CO.CDP.ApplicationRegistry.Shared.Enums;
using CO.CDP.ApplicationRegistry.Shared.Responses;

namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Extension methods for organisation user mapping.
/// </summary>
public static class OrganisationUserMappingExtensions
{
    /// <summary>
    /// Converts a UserOrganisationMembership entity to an OrganisationUserResponse.
    /// </summary>
    /// <param name="membership">The user organisation membership entity.</param>
    /// <param name="includeAssignments">Whether to include application assignments.</param>
    /// <returns>The organisation user response model.</returns>
    public static OrganisationUserResponse ToResponse(
        this UserOrganisationMembership membership,
        bool includeAssignments = false)
    {
        return new OrganisationUserResponse
        {
            MembershipId = membership.Id,
            OrganisationId = membership.OrganisationId,
            UserPrincipalId = membership.UserPrincipalId,
            OrganisationRole = membership.OrganisationRole,
            Status = membership.IsActive ? UserStatus.Active : UserStatus.Pending,
            IsActive = membership.IsActive,
            JoinedAt = membership.JoinedAt,
            InvitedBy = membership.InvitedBy,
            ApplicationAssignments = includeAssignments
                ? membership.ApplicationAssignments.ToResponses(includeDetails: true)
                : null,
            CreatedAt = membership.CreatedAt
        };
    }

    /// <summary>
    /// Converts a collection of UserOrganisationMembership entities to OrganisationUserResponse models.
    /// </summary>
    /// <param name="memberships">The collection of user organisation memberships.</param>
    /// <returns>The collection of organisation user response models.</returns>
    public static IEnumerable<OrganisationUserResponse> ToResponses(
        this IEnumerable<UserOrganisationMembership> memberships)
    {
        return memberships.Select(m => m.ToResponse());
    }
}
