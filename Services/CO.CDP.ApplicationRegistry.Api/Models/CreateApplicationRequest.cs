namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Request model for creating an application.
/// </summary>
public record CreateApplicationRequest
{
    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the unique client identifier.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets or sets the application description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets whether the application is active.
    /// </summary>
    public bool IsActive { get; init; } = true;
}