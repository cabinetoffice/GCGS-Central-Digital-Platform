using System.Globalization;
using System.Text.Json;
using CO.CDP.Authentication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.Authentication.Services;

/// <summary>
/// Manages authentication tokens using ASP.NET Core distributed session (e.g., Redis).
/// Stores Authority tokens in session and retrieves OneLogin tokens from the authentication ticket.
/// </summary>
public class SessionService(IHttpContextAccessor httpContextAccessor) : ISessionManager
{
    /// <summary>
    /// Session key for storing Authority tokens.
    /// </summary>
    public const string AuthorityTokensKey = "AuthorityTokens";

    public Task<AuthorityTokenSet?> GetTokensAsync(HttpContext context)
    {
        if (context.Items.TryGetValue(AuthorityTokensKey, out var item) && item is AuthorityTokenSet cachedTokens)
            return Task.FromResult<AuthorityTokenSet?>(cachedTokens);

        var session = GetSession();
        var tokensJson = session.GetString(AuthorityTokensKey);

        if (string.IsNullOrEmpty(tokensJson))
            return Task.FromResult<AuthorityTokenSet?>(null);

        var tokens = JsonSerializer.Deserialize<AuthorityTokenSet>(tokensJson);

        if (tokens != null)
        {
            context.Items[AuthorityTokensKey] = tokens;
        }

        return Task.FromResult(tokens);
    }

    public Task SetTokensAsync(HttpContext context, AuthorityTokenSet tokens)
    {
        var session = GetSession();
        session.SetString(AuthorityTokensKey, JsonSerializer.Serialize(tokens));

        context.Items[AuthorityTokensKey] = tokens;

        return Task.CompletedTask;
    }

    public Task RemoveTokensAsync(HttpContext context)
    {
        var session = GetSession();
        session.Remove(AuthorityTokensKey);
        context.Items.Remove(AuthorityTokensKey);

        return Task.CompletedTask;
    }

    public async Task<string?> GetOneLoginTokenAsync(HttpContext context)
    {
        return await context.GetTokenAsync("access_token");
    }

    public async Task<DateTimeOffset?> GetOneLoginExpiryAsync(HttpContext context)
    {
        var expiresAt = await context.GetTokenAsync("expires_at");
        return ParseOneLoginExpiry(expiresAt);
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
