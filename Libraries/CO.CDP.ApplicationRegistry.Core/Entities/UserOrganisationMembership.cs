using CO.CDP.ApplicationRegistry.Shared.Enums;

namespace CO.CDP.ApplicationRegistry.Core.Entities;

/// <summary>
/// Represents a user's membership in an organisation.
/// </summary>
public class UserOrganisationMembership : ISoftDelete, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for this membership.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user principal identifier (subject from identity provider).
    /// </summary>
    public string UserPrincipalId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the organisation identifier.
    /// </summary>
    public int OrganisationId { get; set; }

    /// <summary>
    /// Gets or sets the role of the user within the organisation.
    /// </summary>
    public OrganisationRole OrganisationRole { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this membership is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user joined the organisation.
    /// </summary>
    public DateTimeOffset JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who invited this member.
    /// </summary>
    public string? InvitedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // IAuditable
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    // Navigation properties
    public Organisation Organisation { get; set; } = null!;
    public ICollection<UserApplicationAssignment> ApplicationAssignments { get; set; } = new List<UserApplicationAssignment>();
}
