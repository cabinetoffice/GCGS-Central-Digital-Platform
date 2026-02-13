namespace CO.CDP.UserManagement.Core.Entities;

/// <summary>
/// Represents an application role assignment for a pending invite.
/// Stores which application roles should be granted when the invite is accepted.
/// </summary>
public class InviteRoleApplicationAssignment : ISoftDelete, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for this assignment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the invite role mapping identifier.
    /// </summary>
    public int InviteRoleMappingId { get; set; }

    /// <summary>
    /// Gets or sets the organisation application identifier.
    /// </summary>
    public int OrganisationApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the application role identifier.
    /// </summary>
    public int ApplicationRoleId { get; set; }

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
    public InviteRoleMapping InviteRoleMapping { get; set; } = null!;
    public OrganisationApplication OrganisationApplication { get; set; } = null!;
    public ApplicationRole ApplicationRole { get; set; } = null!;
}
