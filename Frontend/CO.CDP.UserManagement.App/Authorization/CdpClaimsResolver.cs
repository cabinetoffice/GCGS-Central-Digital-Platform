using System.IdentityModel.Tokens.Jwt;
using CO.CDP.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.UserManagement.App.Authorization;

/// <summary>
/// Resolves the raw <c>cdp_claims</c> JSON string from a request, checking the
/// ClaimsPrincipal first (populated by <c>OidcEventsService</c>) then falling back
/// to reading the CDP access token directly from the session.
/// </summary>
internal static class CdpClaimsResolver
{
    private const string ClaimType = "cdp_claims";

    internal static async Task<string?> ResolveAsync(
        AuthorizationHandlerContext context,
        HttpContext? httpContext,
        ISessionManager sessionManager,
        ILogger logger)
    {
        var fromPrincipal = context.User.FindFirst(ClaimType)?.Value;
        if (!string.IsNullOrWhiteSpace(fromPrincipal))
            return fromPrincipal;

        if (httpContext is null)
            return null;

        var tokenSet = await sessionManager.GetTokensAsync(httpContext);
        var authorityToken = tokenSet?.AccessToken;

        if (string.IsNullOrWhiteSpace(authorityToken))
            return null;

        try
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(authorityToken);
            return token.Claims.FirstOrDefault(c => c.Type == ClaimType)?.Value;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "CdpClaimsResolver: failed to read JWT or extract {ClaimType}", ClaimType);
            return null;
        }
    }
}
