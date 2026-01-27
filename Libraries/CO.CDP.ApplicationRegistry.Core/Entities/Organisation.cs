namespace CO.CDP.ApplicationRegistry.Core.Entities;

/// <summary>
/// Represents an organisation in the system.
/// </summary>
public class Organisation : ISoftDelete, IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for this organisation.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the CDP organisation GUID that links to the main CDP organisation.
    /// </summary>
    public Guid CdpOrganisationGuid { get; set; }

    /// <summary>
    /// Gets or sets the name of the organisation.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL-friendly slug for the organisation.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this organisation is active.
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
    public ICollection<UserOrganisationMembership> UserMemberships { get; set; } = new List<UserOrganisationMembership>();
    public ICollection<OrganisationApplication> OrganisationApplications { get; set; } = new List<OrganisationApplication>();
}
