namespace CO.CDP.ApplicationRegistry.Api.Models;

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