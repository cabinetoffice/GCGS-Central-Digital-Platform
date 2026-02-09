namespace CO.CDP.UserManagement.Shared.Responses;

/// <summary>
/// Standard error response model.
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Gets or sets additional error details.
    /// </summary>
    public Dictionary<string, string>? Details { get; init; }
}
