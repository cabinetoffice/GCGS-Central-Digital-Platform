namespace CO.CDP.ApplicationRegistry.Core.Entities;

/// <summary>
/// Represents the enablement of an application for an organisation.
/// </summary>
public class OrganisationApplication : ISoftDelete, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for this organisation-application relationship.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the organisation identifier.
    /// </summary>
    public int OrganisationId { get; set; }

    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this application is active for the organisation.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the application was enabled for the organisation.
    /// </summary>
    public DateTimeOffset? EnabledAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who enabled the application.
    /// </summary>
    public string? EnabledBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the application was disabled for the organisation.
    /// </summary>
    public DateTimeOffset? DisabledAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who disabled the application.
    /// </summary>
    public string? DisabledBy { get; set; }

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
    public Application Application { get; set; } = null!;
    public ICollection<UserApplicationAssignment> UserAssignments { get; set; } = new List<UserApplicationAssignment>();
}
