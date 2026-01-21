using CO.CDP.Authentication.Models;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.Authentication;

/// <summary>
/// Manages authentication token storage and retrieval.
/// Handles both Authority tokens (stored in session) and OneLogin tokens (stored in auth ticket).
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Gets the Authority tokens from the distributed session.
    /// </summary>
    Task<AuthorityTokenSet?> GetTokensAsync(HttpContext context);

    /// <summary>
    /// Stores the Authority tokens in the distributed session.
    /// </summary>
    Task SetTokensAsync(HttpContext context, AuthorityTokenSet tokens);

    /// <summary>
    /// Removes the Authority tokens from the distributed session (e.g., on logout).
    /// </summary>
    Task RemoveTokensAsync(HttpContext context);

    /// <summary>
    /// Gets the OneLogin access token from the authentication ticket.
    /// </summary>
    Task<string?> GetOneLoginTokenAsync(HttpContext context);

    /// <summary>
    /// Gets the OneLogin token expiry time from the authentication ticket.
    /// </summary>
    Task<DateTimeOffset?> GetOneLoginExpiryAsync(HttpContext context);
}

/// <summary>
/// Service for exchanging and refreshing tokens with the Authority API.
/// </summary>
public interface ITokenExchangeService
{
    /// <summary>
    /// Exchanges a OneLogin access token for Authority tokens.
    /// </summary>
    Task<AuthorityTokenSet> ExchangeOneLoginTokenAsync(string oneLoginToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes expired Authority tokens using the refresh token.
    /// </summary>
    Task<AuthorityTokenSet> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the refresh token (e.g., on logout).
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

