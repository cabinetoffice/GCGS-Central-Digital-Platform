using System.Globalization;
using System.Text.Json;
using CO.CDP.Authentication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CO.CDP.Authentication.Services;

/// <summary>
/// Manages authentication tokens using ASP.NET Core distributed session (e.g., Redis).
/// Stores Authority tokens in session and retrieves OneLogin tokens from the authentication ticket.
/// </summary>
public class SessionService(
    IHttpContextAccessor httpContextAccessor,
    ILogger<SessionService> logger) : ISessionManager
{
    /// <summary>
    /// Session key for storing Authority tokens. This matches OrganisationApp so
    /// shared Redis-backed sessions can be reused across frontend apps.
    /// </summary>
    public const string AuthorityTokensKey = "UserAuthTokens";

    public Task<AuthorityTokenSet?> GetTokensAsync(HttpContext context)
    {
        if (context.Items.TryGetValue(AuthorityTokensKey, out var item) && item is AuthorityTokenSet cachedTokens)
        {
            logger.LogInformation(
                "Using cached Authority tokens from HttpContext items for session key {SessionKey}. Access token expires at {AccessTokenExpiry}, refresh token expires at {RefreshTokenExpiry}.",
                AuthorityTokensKey,
                cachedTokens.AccessTokenExpiry,
                cachedTokens.RefreshTokenExpiry);

            return Task.FromResult<AuthorityTokenSet?>(cachedTokens);
        }

        var session = GetSession();
        var tokensJson = session.GetString(AuthorityTokensKey);

        if (string.IsNullOrEmpty(tokensJson))
        {
            logger.LogInformation(
                "No Authority tokens found in session key {SessionKey}.",
                AuthorityTokensKey);

            return Task.FromResult<AuthorityTokenSet?>(null);
        }

        var tokens = JsonSerializer.Deserialize<AuthorityTokenSet>(tokensJson);

        if (tokens != null)
        {
            context.Items[AuthorityTokensKey] = tokens;

            logger.LogInformation(
                "Loaded Authority tokens from session key {SessionKey}. Access token expires at {AccessTokenExpiry}, refresh token expires at {RefreshTokenExpiry}.",
                AuthorityTokensKey,
                tokens.AccessTokenExpiry,
                tokens.RefreshTokenExpiry);
        }
        else
        {
            logger.LogWarning(
                "Failed to deserialize Authority tokens from session key {SessionKey}.",
                AuthorityTokensKey);
        }

        return Task.FromResult(tokens);
    }

    public Task SetTokensAsync(HttpContext context, AuthorityTokenSet tokens)
    {
        var session = GetSession();
        session.SetString(AuthorityTokensKey, JsonSerializer.Serialize(tokens));

        context.Items[AuthorityTokensKey] = tokens;

        logger.LogInformation(
            "Stored Authority tokens in session key {SessionKey}. Access token expires at {AccessTokenExpiry}, refresh token expires at {RefreshTokenExpiry}.",
            AuthorityTokensKey,
            tokens.AccessTokenExpiry,
            tokens.RefreshTokenExpiry);

        return Task.CompletedTask;
    }

    public Task RemoveTokensAsync(HttpContext context)
    {
        var session = GetSession();
        session.Remove(AuthorityTokensKey);
        context.Items.Remove(AuthorityTokensKey);

        logger.LogInformation(
            "Removed Authority tokens from session key {SessionKey}.",
            AuthorityTokensKey);

        return Task.CompletedTask;
    }

    public async Task<string?> GetOneLoginTokenAsync(HttpContext context)
    {
        var token = await context.GetTokenAsync("access_token");

        logger.LogInformation(
            "Resolved OneLogin access token from authentication ticket. Present: {HasToken}.",
            !string.IsNullOrWhiteSpace(token));

        return token;
    }

    public async Task<DateTimeOffset?> GetOneLoginExpiryAsync(HttpContext context)
    {
        var expiresAt = await context.GetTokenAsync("expires_at");
        var expiry = ParseOneLoginExpiry(expiresAt);

        logger.LogInformation(
            "Resolved OneLogin expiry from authentication ticket. Present: {HasExpiry}. Expires at: {Expiry}.",
            expiry.HasValue,
            expiry);

        return expiry;
    }

    /// <summary>
    /// Parses the OneLogin expiry timestamp string into a DateTimeOffset.
    /// Returns null if the string is null, empty, or cannot be parsed.
    /// </summary>
    private static DateTimeOffset? ParseOneLoginExpiry(string? expiresAtString)
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

    private ISession GetSession()
    {
        if (httpContextAccessor.HttpContext?.Session == null)
        {
            throw new InvalidOperationException("Session is not available. Ensure session middleware is configured.");
        }
        return httpContextAccessor.HttpContext.Session;
    }
}
