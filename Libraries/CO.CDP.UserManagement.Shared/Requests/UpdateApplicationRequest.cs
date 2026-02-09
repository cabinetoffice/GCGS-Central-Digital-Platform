namespace CO.CDP.UserManagement.Shared.Requests;

/// <summary>
/// Request model for updating an application.
/// </summary>
public record UpdateApplicationRequest
{
    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the application description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets whether the application is active.
    /// </summary>
    public required bool IsActive { get; init; }
}
