namespace CO.CDP.UserManagement.Core.Models;

/// <summary>
/// Represents the complete set of claims for a user across all organisations and applications.
/// </summary>
public record UserClaims
{
    /// <summary>
    /// Gets the user principal identifier.
    /// </summary>
    public required string UserPrincipalId { get; init; }

    /// <summary>
    /// Gets the collection of organisation memberships for the user.
    /// </summary>
    public ICollection<OrganisationMembershipClaim> OrganisationMemberships { get; init; } = new List<OrganisationMembershipClaim>();
}

/// <summary>
/// Represents a user's membership in an organisation with their application assignments.
/// </summary>
public record OrganisationMembershipClaim
{
    /// <summary>
    /// Gets the organisation identifier.
    /// </summary>
    public required int OrganisationId { get; init; }

    /// <summary>
    /// Gets the organisation name.
    /// </summary>
    public required string OrganisationName { get; init; }

    /// <summary>
    /// Gets the organisation slug.
    /// </summary>
    public required string OrganisationSlug { get; init; }

    /// <summary>
    /// Gets the organisation role.
    /// </summary>
    public required string OrganisationRole { get; init; }

    /// <summary>
    /// Gets the CDP person identifier.
    /// </summary>
    public Guid? CdpPersonId { get; init; }

    /// <summary>
    /// Gets the collection of application assignments for this organisation.
    /// </summary>
    public ICollection<ApplicationAssignmentClaim> ApplicationAssignments { get; init; } = new List<ApplicationAssignmentClaim>();
}

/// <summary>
/// Represents a user's assignment to an application with their roles and permissions.
/// </summary>
public record ApplicationAssignmentClaim
{
    /// <summary>
    /// Gets the application identifier.
    /// </summary>
    public required int ApplicationId { get; init; }

    /// <summary>
    /// Gets the application name.
    /// </summary>
    public required string ApplicationName { get; init; }

    /// <summary>
    /// Gets the application client identifier.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets the collection of role names assigned to the user.
    /// </summary>
    public ICollection<string> Roles { get; init; } = new List<string>();

    /// <summary>
    /// Gets the collection of permission names granted to the user.
    /// </summary>
    public ICollection<string> Permissions { get; init; } = new List<string>();
}
