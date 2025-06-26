using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationInformation.Persistence;

/// <summary>
/// Database entity representing a hierarchical relationship between two organisations (parent and child)
/// in the organisation structure.
/// </summary>
public class OrganisationHierarchy
{
    /// <summary>
    /// The unique identifier for this relationship
    /// </summary>
    [Key]
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// The unique identifier of the parent organisation
    /// </summary>
    [Required]
    public Guid ParentId { get; set; }

    /// <summary>
    /// The unique identifier of the child organisation
    /// </summary>
    [Required]
    public Guid ChildId { get; set; }

    /// <summary>
    /// The roles associated with this relationship, stored as a serialized collection
    /// </summary>
    [Required]
    public required  List<PartyRole> Roles { get; set; }

    /// <summary>
    /// The date and time when this relationship was created
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// The date and time when this relationship was deleted/superseded
    /// Null if the relationship is still active
    /// </summary>
    public DateTime? SupersededOn { get; set; }
}
