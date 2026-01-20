using System.Globalization;

namespace CO.CDP.ApplicationRegistry.Core.Tokens;

/// <summary>
/// Pure static functions for making decisions about token state and actions.
/// </summary>
public static class TokenDecisions
{
    /// <summary>
    /// Determines what action to take based on the current token state and OneLogin token availability.
    /// </summary>
    /// <param name="currentTokens">The tokens currently stored in session (if any)</param>
    /// <param name="oneLoginExpiresAt">The parsed expiry time of the OneLogin token (if available)</param>
    /// <param name="oneLoginAccessToken">The OneLogin access token (if available)</param>
    /// <param name="currentTime">The current time (injected for testability)</param>
    /// <returns>The action to take</returns>
    public static TokenAction DetermineAction(
        AuthTokens? currentTokens,
        DateTimeOffset? oneLoginExpiresAt,
        string? oneLoginAccessToken,
        DateTimeOffset currentTime)
    {
        if (currentTokens == null)
        {
            if (oneLoginExpiresAt.HasValue && oneLoginExpiresAt.Value < currentTime)
            {
                return new TokenAction.OneLoginExpired(oneLoginExpiresAt.Value);
            }

            if (string.IsNullOrWhiteSpace(oneLoginAccessToken))
            {
                return new TokenAction.UserLoggedOut();
            }

            return new TokenAction.FetchNew(oneLoginAccessToken);
        }

        if (currentTokens.RefreshTokenExpiry < currentTime)
        {
            if (string.IsNullOrWhiteSpace(oneLoginAccessToken))
            {
                return new TokenAction.UserLoggedOut();
            }

            return new TokenAction.FetchNew(oneLoginAccessToken);
        }

        if (currentTokens.AccessTokenExpiry < currentTime && currentTokens.RefreshTokenExpiry >= currentTime)
        {
            return new TokenAction.RefreshTokens(currentTokens.RefreshToken);
        }

        return new TokenAction.UseExisting(currentTokens);
    }

    /// <summary>
    /// Parses the OneLogin expiry timestamp string into a DateTimeOffset.
    /// Returns null if the string is null, empty, or cannot be parsed.
    /// </summary>
    public static DateTimeOffset? ParseOneLoginExpiry(string? expiresAtString)
    {
        if (string.IsNullOrEmpty(expiresAtString))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(expiresAtString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiresAt))
        {
            return expiresAt.ToLocalTime();
        }

        return null;
    }

    /// <summary>
    /// Creates AuthTokens from a token response, calculating expiry times based on current time.
    /// </summary>
    /// <param name="accessToken">The access token</param>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="expiresInSeconds">Seconds until access token expires</param>
    /// <param name="refreshExpiresInSeconds">Seconds until refresh token expires</param>
    /// <param name="currentTime">The current time (injected for testability)</param>
    /// <param name="bufferSeconds">Safety buffer to subtract from expiry (default 30 seconds)</param>
    /// <returns>AuthTokens with calculated expiry times</returns>
    public static AuthTokens CreateAuthTokens(
        string accessToken,
        string refreshToken,
        double expiresInSeconds,
        double refreshExpiresInSeconds,
        DateTimeOffset currentTime,
        int bufferSeconds = 30)
    {
        return new AuthTokens
        {
            AccessToken = accessToken,
            AccessTokenExpiry = currentTime.AddSeconds(expiresInSeconds - bufferSeconds),
            RefreshToken = refreshToken,
            RefreshTokenExpiry = currentTime.AddSeconds(refreshExpiresInSeconds - bufferSeconds)
        };
    }

    /// <summary>
    /// Determines if a refresh token should be revoked based on its expiry time.
    /// </summary>
    /// <param name="tokens">The tokens to check (if any)</param>
    /// <param name="currentTime">The current time (injected for testability)</param>
    /// <returns>True if the refresh token is present and not yet expired</returns>
    public static bool ShouldRevokeRefreshToken(AuthTokens? tokens, DateTimeOffset currentTime)
    {
        if (tokens == null)
        {
            return false;
        }

        return tokens.RefreshTokenExpiry > currentTime;
    }
}

