using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Request model for assigning a user to an application with roles.
/// </summary>
public record AssignUserToApplicationRequest
{
    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    public required int ApplicationId { get; init; }

    /// <summary>
    /// Gets or sets the collection of role identifiers to assign.
    /// </summary>
    public required IEnumerable<int> RoleIds { get; init; }
}

/// <summary>
/// Request model for updating a user's assignment roles.
/// </summary>
public record UpdateAssignmentRolesRequest
{
    /// <summary>
    /// Gets or sets the collection of role identifiers.
    /// </summary>
    public required IEnumerable<int> RoleIds { get; init; }
}

/// <summary>
/// Response model for a user application assignment.
/// </summary>
public record UserAssignmentResponse
{
    /// <summary>
    /// Gets or sets the assignment identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets or sets the user organisation membership identifier.
    /// </summary>
    public required int UserOrganisationMembershipId { get; init; }

    /// <summary>
    /// Gets or sets the user principal identifier.
    /// </summary>
    public string? UserPrincipalId { get; init; }

    /// <summary>
    /// Gets or sets the organisation application identifier.
    /// </summary>
    public required int OrganisationApplicationId { get; init; }

    /// <summary>
    /// Gets or sets the organisation identifier.
    /// </summary>
    public int? OrganisationId { get; init; }

    /// <summary>
    /// Gets or sets the organisation details.
    /// </summary>
    public OrganisationSummaryResponse? Organisation { get; init; }

    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    public int? ApplicationId { get; init; }

    /// <summary>
    /// Gets or sets the application details.
    /// </summary>
    public ApplicationSummaryResponse? Application { get; init; }

    /// <summary>
    /// Gets or sets the collection of roles assigned to the user.
    /// </summary>
    public IEnumerable<RoleResponse>? Roles { get; init; }

    /// <summary>
    /// Gets or sets whether this assignment is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the user was assigned.
    /// </summary>
    public DateTimeOffset? AssignedAt { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the user who made the assignment.
    /// </summary>
    public string? AssignedBy { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the assignment was revoked.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the user who revoked the assignment.
    /// </summary>
    public string? RevokedBy { get; init; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}

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
