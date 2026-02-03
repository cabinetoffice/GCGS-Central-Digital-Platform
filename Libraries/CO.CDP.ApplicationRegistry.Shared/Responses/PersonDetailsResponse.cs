namespace CO.CDP.ApplicationRegistry.Shared.Responses;

/// <summary>
/// Response model for person details retrieved from CDP Person service.
/// </summary>
public record PersonDetailsResponse
{
    /// <summary>
    /// Gets the person's first name.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Gets the person's last name.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Gets the person's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the person's user URN (user principal identifier).
    /// </summary>
    public required string UserUrn { get; init; }
}
