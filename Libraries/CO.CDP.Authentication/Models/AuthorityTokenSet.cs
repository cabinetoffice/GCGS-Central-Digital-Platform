namespace CO.CDP.Authentication.Models;

/// <summary>
/// Value object representing Organisation Authority authentication tokens with their expiry times.
/// Encapsulates token state and provides domain behavior for token lifecycle management.
/// </summary>
public sealed record AuthorityTokenSet
{
    public required string AccessToken { get; init; }

    public required DateTimeOffset AccessTokenExpiry { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset RefreshTokenExpiry { get; init; }

    /// <summary>
    /// Factory method to create an AuthorityTokenSet from a token response, calculating expiry times.
    /// </summary>
    /// <param name="accessToken">The access token</param>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="expiresInSeconds">Seconds until access token expires</param>
    /// <param name="refreshExpiresInSeconds">Seconds until refresh token expires</param>
    /// <param name="currentTime">The current time (injected for testability)</param>
    /// <param name="bufferSeconds">Safety buffer to subtract from expiry (default 30 seconds)</param>
    /// <returns>AuthorityTokenSet with calculated expiry times</returns>
    public static AuthorityTokenSet Create(
        string accessToken,
        string refreshToken,
        double expiresInSeconds,
        double refreshExpiresInSeconds,
        DateTimeOffset currentTime,
        int bufferSeconds = 30)
    {
        return new AuthorityTokenSet
        {
            AccessToken = accessToken,
            AccessTokenExpiry = currentTime.AddSeconds(expiresInSeconds - bufferSeconds),
            RefreshToken = refreshToken,
            RefreshTokenExpiry = currentTime.AddSeconds(refreshExpiresInSeconds - bufferSeconds)
        };
    }

    /// <summary>
    /// Determines if the access token has expired.
    /// </summary>
    public bool IsAccessTokenExpired(DateTimeOffset currentTime)
        => AccessTokenExpiry < currentTime;

    /// <summary>
    /// Determines if the refresh token has expired.
    /// </summary>
    public bool IsRefreshTokenExpired(DateTimeOffset currentTime)
        => RefreshTokenExpiry < currentTime;

    /// <summary>
    /// Determines if the refresh token should be revoked (i.e., it's still valid).
    /// </summary>
    public bool ShouldRevokeRefreshToken(DateTimeOffset currentTime)
        => RefreshTokenExpiry > currentTime;
}