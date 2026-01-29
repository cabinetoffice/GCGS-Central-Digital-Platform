namespace CO.CDP.ApplicationRegistry.Shared.Responses;

/// <summary>
/// Response model for an application.
/// </summary>
public record ApplicationResponse
{
    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets or sets the application description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the application category.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets or sets whether the application is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
