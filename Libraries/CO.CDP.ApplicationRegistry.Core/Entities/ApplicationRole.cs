namespace CO.CDP.ApplicationRegistry.Core.Entities;

/// <summary>
/// Represents a role within an application that can have multiple permissions.
/// </summary>
public class ApplicationRole : ISoftDelete, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for this role.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the application identifier this role belongs to.
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the role.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this role is active.
    /// </summary>
    public bool IsActive { get; set; }

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
    public Application Application { get; set; } = null!;
    public ICollection<ApplicationPermission> Permissions { get; set; } = new List<ApplicationPermission>();
    public ICollection<UserApplicationAssignment> UserAssignments { get; set; } = new List<UserApplicationAssignment>();
}
