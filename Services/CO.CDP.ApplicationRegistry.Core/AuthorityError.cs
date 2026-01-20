namespace CO.CDP.ApplicationRegistry.Core;

/// <summary>
/// Discriminated union representing errors that can occur during authority token operations.
/// </summary>
public abstract record AuthorityError
{
    /// <summary>
    /// The OneLogin token has expired and cannot be used to fetch new tokens.
    /// </summary>
    public sealed record OneLoginExpired(DateTimeOffset ExpiryTime) : AuthorityError;

    /// <summary>
    /// The user is logged out and has no valid OneLogin access token.
    /// </summary>
    public sealed record UserLoggedOut : AuthorityError;

    /// <summary>
    /// An HTTP request to the authority service failed.
    /// </summary>
    public sealed record HttpRequestFailed(int StatusCode, string Message) : AuthorityError;

    /// <summary>
    /// The response from the authority service was invalid or could not be parsed.
    /// </summary>
    public sealed record InvalidResponse(string Message) : AuthorityError;
}

