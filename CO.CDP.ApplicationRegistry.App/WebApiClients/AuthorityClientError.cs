namespace CO.CDP.ApplicationRegistry.App.WebApiClients;

/// <summary>
/// Represents errors that can occur during authority client operations.
/// </summary>
public abstract record AuthorityClientError
{
    /// <summary>
    /// User is not authenticated.
    /// </summary>
    public sealed record NoUser : AuthorityClientError;

    /// <summary>
    /// OneLogin token has expired.
    /// </summary>
    public sealed record OneLoginTokenExpired(DateTimeOffset ExpiryTime) : AuthorityClientError;

    /// <summary>
    /// User is logged out (no OneLogin access token).
    /// </summary>
    public sealed record UserLoggedOut : AuthorityClientError;

    /// <summary>
    /// HTTP request to authority service failed.
    /// </summary>
    public sealed record HttpRequestFailed(string Message) : AuthorityClientError;

    /// <summary>
    /// Token response is invalid or missing required fields.
    /// </summary>
    public sealed record InvalidTokenResponse(string Message) : AuthorityClientError;

    /// <summary>
    /// Token revocation failed.
    /// </summary>
    public sealed record TokenRevocationFailed(string Message) : AuthorityClientError;
}

