namespace CO.CDP.ApplicationRegistry.Shared.Requests;

/// <summary>
/// Request model for enabling an application for an organisation.
/// </summary>
public record EnableApplicationRequest
{
    /// <summary>
    /// Gets or sets the application identifier to enable.
    /// </summary>
    public required int ApplicationId { get; init; }
}
