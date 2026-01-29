namespace CO.CDP.ApplicationRegistry.Core.Entities;

/// <summary>
/// Represents an application in the system that organisations can enable.
/// </summary>
public class Application : ISoftDelete, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for this application.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique client identifier for this application.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the application.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the category of the application.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this application is active.
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
    public ICollection<ApplicationPermission> Permissions { get; set; } = new List<ApplicationPermission>();
    public ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
    public ICollection<OrganisationApplication> OrganisationApplications { get; set; } = new List<OrganisationApplication>();
}
