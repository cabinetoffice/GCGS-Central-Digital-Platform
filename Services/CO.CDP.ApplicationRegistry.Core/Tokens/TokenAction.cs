namespace CO.CDP.ApplicationRegistry.Core.Tokens;

/// <summary>
/// Discriminated union representing the action to take based on token state analysis.
/// This makes the decision tree explicit and testable.
/// </summary>
public abstract record TokenAction
{
    /// <summary>
    /// No user is authenticated (userUrn is null).
    /// </summary>
    public sealed record NoUser : TokenAction;

    /// <summary>
    /// Use the existing valid tokens from session.
    /// </summary>
    public sealed record UseExisting(AuthTokens Tokens) : TokenAction;

    /// <summary>
    /// Fetch new tokens using the OneLogin access token.
    /// </summary>
    public sealed record FetchNew(string OneLoginAccessToken) : TokenAction;

    /// <summary>
    /// Refresh the expired access token using the refresh token.
    /// </summary>
    public sealed record RefreshTokens(string RefreshToken) : TokenAction;

    /// <summary>
    /// The OneLogin token has expired and cannot be used.
    /// </summary>
    public sealed record OneLoginExpired(DateTimeOffset ExpiryTime) : TokenAction;

    /// <summary>
    /// The user is logged out (no OneLogin access token available).
    /// </summary>
    public sealed record UserLoggedOut : TokenAction;
}

