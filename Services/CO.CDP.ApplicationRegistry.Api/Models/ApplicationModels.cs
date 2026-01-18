using CO.CDP.ApplicationRegistry.Core.Entities;

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
    /// Gets or sets whether the application is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Summary response model for an application (reduced details).
/// </summary>
public record ApplicationSummaryResponse
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
    /// Gets or sets whether the application is active.
    /// </summary>
    public required bool IsActive { get; init; }
}

/// <summary>
/// Extension methods for application mapping.
/// </summary>
public static class ApplicationMappingExtensions
{
    public static ApplicationResponse ToResponse(this Application application)
    {
        return new ApplicationResponse
        {
            Id = application.Id,
            Name = application.Name,
            ClientId = application.ClientId,
            Description = application.Description,
            IsActive = application.IsActive,
            CreatedAt = application.CreatedAt
        };
    }

    public static ApplicationSummaryResponse ToSummaryResponse(this Application application)
    {
        return new ApplicationSummaryResponse
        {
            Id = application.Id,
            Name = application.Name,
            ClientId = application.ClientId,
            IsActive = application.IsActive
        };
    }
}
