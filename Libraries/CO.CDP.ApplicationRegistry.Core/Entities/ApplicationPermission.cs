namespace CO.CDP.ApplicationRegistry.Core.Entities;

/// <summary>
/// Represents a permission within an application.
/// </summary>
public class ApplicationPermission : ISoftDelete, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for this permission.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the application identifier this permission belongs to.
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the name of the permission.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the permission.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this permission is active.
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
    public ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
}
