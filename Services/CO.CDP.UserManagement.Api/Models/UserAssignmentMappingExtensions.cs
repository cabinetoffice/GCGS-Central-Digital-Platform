using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.ApplicationRegistry.Shared.Responses;

namespace CO.CDP.UserManagement.Api.Models;

/// <summary>
/// Extension methods for user assignment mapping.
/// </summary>
public static class UserAssignmentMappingExtensions
{
    /// <summary>
    /// Converts a UserApplicationAssignment entity to a UserAssignmentResponse.
    /// </summary>
    /// <param name="assignment">The user application assignment entity.</param>
    /// <param name="includeDetails">Whether to include organisation, application, and role details.</param>
    /// <returns>The user assignment response model.</returns>
    public static UserAssignmentResponse ToResponse(
        this UserApplicationAssignment assignment,
        bool includeDetails = true)
    {
        var orgApp = assignment.OrganisationApplication;
        var membership = assignment.UserOrganisationMembership;

        return new UserAssignmentResponse
        {
            Id = assignment.Id,
            UserOrganisationMembershipId = assignment.UserOrganisationMembershipId,
            UserPrincipalId = includeDetails && membership != null ? membership.UserPrincipalId : null,
            OrganisationApplicationId = assignment.OrganisationApplicationId,
            OrganisationId = includeDetails && orgApp != null ? orgApp.OrganisationId : null,
            Organisation = includeDetails && orgApp?.Organisation != null
                ? orgApp.Organisation.ToSummaryResponse()
                : null,
            ApplicationId = includeDetails && orgApp != null ? orgApp.ApplicationId : null,
            Application = includeDetails && orgApp?.Application != null
                ? orgApp.Application.ToSummaryResponse()
                : null,
            Roles = includeDetails && assignment.Roles != null
                ? assignment.Roles.Select(r => r.ToResponse(includePermissions: false))
                : null,
            IsActive = assignment.IsActive,
            AssignedAt = assignment.AssignedAt,
            AssignedBy = assignment.AssignedBy,
            RevokedAt = assignment.RevokedAt,
            RevokedBy = assignment.RevokedBy,
            CreatedAt = assignment.CreatedAt
        };
    }

    /// <summary>
    /// Converts a collection of UserApplicationAssignment entities to UserAssignmentResponse models.
    /// </summary>
    /// <param name="assignments">The collection of user application assignment entities.</param>
    /// <param name="includeDetails">Whether to include organisation, application, and role details.</param>
    /// <returns>The collection of user assignment response models.</returns>
    public static IEnumerable<UserAssignmentResponse> ToResponses(
        this IEnumerable<UserApplicationAssignment> assignments,
        bool includeDetails = true)
    {
        return assignments.Select(a => a.ToResponse(includeDetails));
    }
}