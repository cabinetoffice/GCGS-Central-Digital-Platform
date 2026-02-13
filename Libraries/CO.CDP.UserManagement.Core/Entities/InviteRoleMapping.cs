using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Core.Entities;

/// <summary>
/// Represents a mapping between a CDP PersonInvite and UserManagement-specific role data.
/// This is a thin reference table that stores only the role assignments while the invite
/// lifecycle (email, name, expiry, status) remains in CDP's person_invites table.
/// </summary>
public class InviteRoleMapping : ISoftDelete, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for this mapping.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the CDP PersonInvite GUID for cross-referencing.
    /// This is a reference-only field (not a foreign key) following the
    /// cross-domain reference pattern used throughout the codebase.
    /// </summary>
    public Guid CdpPersonInviteGuid { get; set; }

    /// <summary>
    /// Gets or sets the organisation identifier.
    /// This is an internal foreign key to UserManagement's organisations table.
    /// </summary>
    public int OrganisationId { get; set; }

    /// <summary>
    /// Gets or sets the role for the invited user within the organisation.
    /// </summary>
    public OrganisationRole OrganisationRole { get; set; }

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
    public ICollection<InviteRoleApplicationAssignment> ApplicationAssignments { get; set; } = new List<InviteRoleApplicationAssignment>();
}
