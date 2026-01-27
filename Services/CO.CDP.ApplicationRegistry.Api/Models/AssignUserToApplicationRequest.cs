namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Request model for assigning a user to an application with roles.
/// </summary>
public record AssignUserToApplicationRequest
{
    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    public required int ApplicationId { get; init; }

    /// <summary>
    /// Gets or sets the collection of role identifiers to assign.
    /// </summary>
    public required IEnumerable<int> RoleIds { get; init; }
}