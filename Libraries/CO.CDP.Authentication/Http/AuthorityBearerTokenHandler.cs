using System.Net.Http.Headers;
using CO.CDP.Authentication.Models;
using CO.CDP.Authentication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CO.CDP.Authentication.Http;

public class AuthorityBearerTokenHandler(
    ISessionManager sessionManager,
    ITokenExchangeService tokenExchangeService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthorityBearerTokenHandler> logger)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = await GetValidTokenAsync(httpContext, cancellationToken);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string?> GetValidTokenAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var currentTokens = await sessionManager.GetTokensAsync(httpContext);
        var oneLoginToken = await sessionManager.GetOneLoginTokenAsync(httpContext);
        var oneLoginExpiry = await sessionManager.GetOneLoginExpiryAsync(httpContext);

        var action = TokenLifecycleService.DetermineAction(
            currentTokens,
            oneLoginExpiry,
            oneLoginToken,
            DateTimeOffset.UtcNow
        );

        return action switch
        {
            TokenAction.UseExisting useExisting => useExisting.Tokens.AccessToken,

            TokenAction.RefreshTokens refreshTokens =>
                await RefreshTokensAsync(httpContext, refreshTokens.RefreshToken, cancellationToken),

            TokenAction.FetchNew fetchNew =>
                await FetchNewTokensAsync(httpContext, fetchNew.OneLoginAccessToken, cancellationToken),

            TokenAction.UserLoggedOut or TokenAction.OneLoginExpired =>
                await HandleLoggedOutAsync(httpContext),

            _ => null
        };
    }

    private async Task<string?> RefreshTokensAsync(HttpContext httpContext, string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var newTokens = await tokenExchangeService.RefreshTokensAsync(refreshToken, cancellationToken);
            await sessionManager.SetTokensAsync(httpContext, newTokens);
            return newTokens.AccessToken;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to refresh tokens");
            return null;
        }
    }

    private async Task<string?> FetchNewTokensAsync(HttpContext httpContext, string oneLoginToken, CancellationToken cancellationToken)
    {
        try
        {
            var newTokens = await tokenExchangeService.ExchangeOneLoginTokenAsync(oneLoginToken, cancellationToken);
            await sessionManager.SetTokensAsync(httpContext, newTokens);
            return newTokens.AccessToken;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to exchange OneLogin token");
            return null;
        }
    }

    private Task<string?> HandleLoggedOutAsync(HttpContext httpContext)
    {
        _ = httpContext;
        logger.LogInformation("User is logged out or OneLogin token expired");
        return Task.FromResult<string?>(null);
    }
}
