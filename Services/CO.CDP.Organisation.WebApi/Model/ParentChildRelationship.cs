using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

/// <summary>
/// Request model for creating a parent-child relationship between organisations
/// </summary>
public class CreateParentChildRelationshipRequest
{
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
}

/// <summary>
/// Result model for parent-child relationship operations
/// </summary>
public class CreateParentChildRelationshipResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The unique identifier of the relationship, if created
    /// </summary>
    public Guid? RelationshipId { get; set; }
}