namespace CO.CDP.ApplicationRegistry.Core.Entities;

/// <summary>
/// Represents a user's assignment to an application within an organisation with specific roles.
/// </summary>
public class UserApplicationAssignment : ISoftDelete, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for this assignment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user organisation membership identifier.
    /// </summary>
    public int UserOrganisationMembershipId { get; set; }

    /// <summary>
    /// Gets or sets the organisation application identifier.
    /// </summary>
    public int OrganisationApplicationId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this assignment is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was assigned to the application.
    /// </summary>
    public DateTimeOffset? AssignedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who made the assignment.
    /// </summary>
    public string? AssignedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the assignment was revoked.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who revoked the assignment.
    /// </summary>
    public string? RevokedBy { get; set; }

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
    public UserOrganisationMembership UserOrganisationMembership { get; set; } = null!;
    public OrganisationApplication OrganisationApplication { get; set; } = null!;
    public ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
}
