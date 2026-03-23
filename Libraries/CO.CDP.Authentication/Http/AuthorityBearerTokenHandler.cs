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
            logger.LogWarning(
                "No HttpContext available while preparing bearer token for outbound request {Method} {RequestUri}.",
                request.Method,
                request.RequestUri);

            return await base.SendAsync(request, cancellationToken);
        }

        var token = await GetValidTokenAsync(httpContext, cancellationToken);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            logger.LogWarning(
                "No Authority bearer token available for outbound request {Method} {RequestUri}. Request will be sent without an Authorization header.",
                request.Method,
                request.RequestUri);
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

        logger.LogInformation(
            "Evaluated Authority token lifecycle. Action: {Action}. HasStoredAuthorityTokens: {HasStoredAuthorityTokens}. HasOneLoginToken: {HasOneLoginToken}. OneLoginExpiry: {OneLoginExpiry}.",
            action.GetType().Name,
            currentTokens != null,
            !string.IsNullOrWhiteSpace(oneLoginToken),
            oneLoginExpiry);

        return action switch
        {
            TokenAction.UseExisting useExisting => LogAndReturnExistingToken(useExisting),

            TokenAction.RefreshTokens refreshTokens =>
                await RefreshTokensAsync(httpContext, refreshTokens.RefreshToken, cancellationToken),

            TokenAction.FetchNew fetchNew =>
                await FetchNewTokensAsync(httpContext, fetchNew.OneLoginAccessToken, cancellationToken),

            TokenAction.UserLoggedOut =>
                await HandleLoggedOutAsync(),

            TokenAction.OneLoginExpired oneLoginExpired =>
                await HandleOneLoginExpiredAsync(oneLoginExpired.ExpiryTime),

            _ => null
        };
    }

    private string LogAndReturnExistingToken(TokenAction.UseExisting useExisting)
    {
        logger.LogInformation(
            "Using existing Authority access token. Access token expires at {AccessTokenExpiry}, refresh token expires at {RefreshTokenExpiry}.",
            useExisting.Tokens.AccessTokenExpiry,
            useExisting.Tokens.RefreshTokenExpiry);

        return useExisting.Tokens.AccessToken;
    }

    private async Task<string?> RefreshTokensAsync(HttpContext httpContext, string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var newTokens = await tokenExchangeService.RefreshTokensAsync(refreshToken, cancellationToken);
            await sessionManager.SetTokensAsync(httpContext, newTokens);

            logger.LogInformation(
                "Refreshed Authority tokens successfully. Access token expires at {AccessTokenExpiry}, refresh token expires at {RefreshTokenExpiry}.",
                newTokens.AccessTokenExpiry,
                newTokens.RefreshTokenExpiry);

            return newTokens.AccessToken;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to refresh Authority tokens.");
            return null;
        }
    }

    private async Task<string?> FetchNewTokensAsync(HttpContext httpContext, string oneLoginToken, CancellationToken cancellationToken)
    {
        try
        {
            var newTokens = await tokenExchangeService.ExchangeOneLoginTokenAsync(oneLoginToken, cancellationToken);
            await sessionManager.SetTokensAsync(httpContext, newTokens);

            logger.LogInformation(
                "Exchanged OneLogin token for Authority tokens successfully. Access token expires at {AccessTokenExpiry}, refresh token expires at {RefreshTokenExpiry}.",
                newTokens.AccessTokenExpiry,
                newTokens.RefreshTokenExpiry);

            return newTokens.AccessToken;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to exchange OneLogin token for Authority tokens.");
            return null;
        }
    }

    private Task<string?> HandleLoggedOutAsync()
    {
        logger.LogInformation("No OneLogin token is available in the current authenticated session.");
        return Task.FromResult<string?>(null);
    }

    private Task<string?> HandleOneLoginExpiredAsync(DateTimeOffset expiresAt)
    {
        logger.LogInformation(
            "OneLogin token is expired. Expired at {ExpiresAt}.",
            expiresAt);

        return Task.FromResult<string?>(null);
    }
}
