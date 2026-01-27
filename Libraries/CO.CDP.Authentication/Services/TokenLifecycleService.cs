using CO.CDP.Authentication.Models;

namespace CO.CDP.Authentication.Services;

/// <summary>
/// Domain service for token lifecycle policy decisions.
/// </summary>
public static class TokenLifecycleService
{
    /// <summary>
    /// Determines what action to take based on the current token state and OneLogin token availability.
    /// </summary>
    /// <param name="currentTokens">The Authority tokens currently stored in session (if any)</param>
    /// <param name="oneLoginExpiresAt">The parsed expiry time of the OneLogin token (if available)</param>
    /// <param name="oneLoginAccessToken">The OneLogin access token (if available)</param>
    /// <param name="currentTime">The current time (injected for testability)</param>
    /// <returns>The action to take</returns>
    public static TokenAction DetermineAction(
        AuthorityTokenSet? currentTokens,
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

        if (currentTokens.IsRefreshTokenExpired(currentTime))
        {
            if (string.IsNullOrWhiteSpace(oneLoginAccessToken))
            {
                return new TokenAction.UserLoggedOut();
            }

            return new TokenAction.FetchNew(oneLoginAccessToken);
        }

        if (currentTokens.IsAccessTokenExpired(currentTime))
        {
            return new TokenAction.RefreshTokens(currentTokens.RefreshToken);
        }

        return new TokenAction.UseExisting(currentTokens);
    }

    /// <summary>
    /// Determines if a refresh token should be revoked based on its expiry time.
    /// </summary>
    /// <param name="tokens">The tokens to check (if any)</param>
    /// <param name="currentTime">The current time (injected for testability)</param>
    /// <returns>True if the refresh token is present and not yet expired</returns>
    public static bool ShouldRevokeRefreshToken(AuthorityTokenSet? tokens, DateTimeOffset currentTime)
    {
        return tokens != null && tokens.RefreshTokenExpiry > currentTime;
    }
}
