using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.OrganisationInformation.Persistence;

/// <summary>
/// Database entity representing a hierarchical relationship between two organisations (parent and child)
/// in the organisation structure.
/// </summary>
public class OrganisationHierarchy
{
    /// <summary>
    /// The primary key for this relationship - auto-incrementing integer
    /// </summary>
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// The unique identifier for this relationship
    /// </summary>
    [Required]
    public required Guid RelationshipId { get; set; }

    /// <summary>
    /// The ID of the parent organisation
    /// </summary>
    [Required]
    [ForeignKey(nameof(Parent))]
    public required int ParentOrganisationId { get; set; }

    /// <summary>
    /// The parent organisation in this relationship
    /// </summary>
    public virtual Organisation? Parent { get; set; }

    /// <summary>
    /// The ID of the child organisation
    /// </summary>
    [Required]
    [ForeignKey(nameof(Child))]
    public int ChildOrganisationId { get; set; }

    /// <summary>
    /// The child organisation in this relationship
    /// </summary>
    public virtual Organisation? Child { get; set; }

    /// <summary>
    /// The roles associated with this relationship, stored as a serialized collection
    /// </summary>
    [Required]
    public required List<PartyRole> Roles { get; set; }

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
