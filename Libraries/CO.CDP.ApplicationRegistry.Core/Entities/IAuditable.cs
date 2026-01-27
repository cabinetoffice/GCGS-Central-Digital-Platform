namespace CO.CDP.ApplicationRegistry.Core.Entities;

/// <summary>
/// Interface for entities that track creation and modification audit information.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// </summary>
    DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    string? ModifiedBy { get; set; }
}
