using CO.CDP.ApplicationRegistry.App.Authentication;
using CO.CDP.ApplicationRegistry.Core.Tokens;
using CO.CDP.Functional;
using IdentityModel;
using IdentityModel.Client;

namespace CO.CDP.ApplicationRegistry.App.WebApiClients;

/// <summary>
/// Client for interacting with the Organisation Authority service.
/// </summary>
public class AuthorityClient(
    IAppSession session,
    ITokenService tokenService,
    IHttpClientFactory httpClientFactory,
    TimeProvider timeProvider) : IAuthorityClient
{
    public const string OrganisationAuthorityHttpClientName = "OrganisationAuthorityHttpClient";

    /// <summary>
    /// Gets authentication tokens for the specified user.
    /// </summary>
    public async Task<Result<AuthorityClientError, AuthTokens>> GetAuthTokens(string? userUrn)
    {
        if (userUrn == null)
        {
            return Result<AuthorityClientError, AuthTokens>.Failure(new AuthorityClientError.NoUser());
        }

        var currentTokens = session.Get<AuthTokens>(Session.UserAuthTokens);
        var expiresAtString = await tokenService.GetTokenAsync("expires_at");
        var oneLoginAccessToken = await tokenService.GetTokenAsync("access_token");

        var oneLoginExpiresAt = TokenDecisions.ParseOneLoginExpiry(expiresAtString);
        var action = TokenDecisions.DetermineAction(
            currentTokens,
            oneLoginExpiresAt,
            oneLoginAccessToken,
            timeProvider.GetUtcNow());

        return await ExecuteTokenAction(action);
    }

    /// <summary>
    /// Revokes the refresh token for the specified user.
    /// </summary>
    public async Task<Result<AuthorityClientError, Unit>> RevokeRefreshToken(string? userUrn)
    {
        if (userUrn == null)
        {
            return Result<AuthorityClientError, Unit>.Failure(new AuthorityClientError.NoUser());
        }

        var tokens = session.Get<AuthTokens>(Session.UserAuthTokens);
        session.Remove(Session.UserAuthTokens);

        if (!TokenDecisions.ShouldRevokeRefreshToken(tokens, timeProvider.GetUtcNow()))
        {
            return Result<AuthorityClientError, Unit>.Success(Unit.Value);
        }

        return await RevokeToken(tokens!.RefreshToken);
    }

    /// <summary>
    /// Executes the determined token action and updates session state accordingly.
    /// </summary>
    private async Task<Result<AuthorityClientError, AuthTokens>> ExecuteTokenAction(TokenAction action)
    {
        return action switch
        {
            TokenAction.UseExisting existing =>
                Result<AuthorityClientError, AuthTokens>.Success(existing.Tokens),

            TokenAction.FetchNew fetchNew =>
                await FetchAndStoreNewTokens(fetchNew.OneLoginAccessToken),

            TokenAction.RefreshTokens refresh =>
                await RefreshAndStoreTokens(refresh.RefreshToken),

            TokenAction.OneLoginExpired expired =>
                Result<AuthorityClientError, AuthTokens>.Failure(new AuthorityClientError.OneLoginTokenExpired(expired.ExpiryTime)),

            TokenAction.UserLoggedOut =>
                Result<AuthorityClientError, AuthTokens>.Failure(new AuthorityClientError.UserLoggedOut()),

            _ => Result<AuthorityClientError, AuthTokens>.Failure(
                new AuthorityClientError.InvalidTokenResponse($"Unknown token action: {action.GetType().Name}"))
        };
    }

    /// <summary>
    /// Fetches new tokens using OneLogin access token and stores them in session.
    /// </summary>
    private async Task<Result<AuthorityClientError, AuthTokens>> FetchAndStoreNewTokens(string oneLoginAccessToken)
    {
        var result = await FetchTokensAsync(OidcConstants.GrantTypes.ClientCredentials, OidcConstants.TokenRequest.ClientSecret, oneLoginAccessToken);
        return result.Match(
            Result<AuthorityClientError, AuthTokens>.Failure,
            tokens =>
            {
                session.Set(Session.UserAuthTokens, tokens);
                return Result<AuthorityClientError, AuthTokens>.Success(tokens);
            });
    }

    /// <summary>
    /// Refreshes tokens using the refresh token and stores them in session.
    /// </summary>
    private async Task<Result<AuthorityClientError, AuthTokens>> RefreshAndStoreTokens(string refreshToken)
    {
        var result = await FetchTokensAsync(OidcConstants.GrantTypes.RefreshToken, OidcConstants.TokenRequest.RefreshToken, refreshToken);
        return result.Match(
            Result<AuthorityClientError, AuthTokens>.Failure,
            tokens =>
            {
                session.Set(Session.UserAuthTokens, tokens);
                return Result<AuthorityClientError, AuthTokens>.Success(tokens);
            });
    }

    /// <summary>
    /// Makes an HTTP request to fetch tokens from the authority service.
    /// </summary>
    private async Task<Result<AuthorityClientError, AuthTokens>> FetchTokensAsync(string grantType, string tokenRequestType, string credential)
    {
        var response = await GetClient().RequestTokenAsync(new TokenRequest
        {
            Address = "/token",
            GrantType = grantType,
            Parameters = { { tokenRequestType, credential } }
        });

        if (response.IsError)
        {
            return Result<AuthorityClientError, AuthTokens>.Failure(
                new AuthorityClientError.HttpRequestFailed($"Unable to get access token from Organisation Authority: {response.Error}"));
        }

        if (string.IsNullOrEmpty(response.AccessToken))
        {
            return Result<AuthorityClientError, AuthTokens>.Failure(
                new AuthorityClientError.InvalidTokenResponse("Token response missing access_token"));
        }

        if (string.IsNullOrEmpty(response.RefreshToken))
        {
            return Result<AuthorityClientError, AuthTokens>.Failure(
                new AuthorityClientError.InvalidTokenResponse("Token response missing refresh_token"));
        }

        var refreshExpiresIn = response.TryGet("refresh_expires_in")
            .TryParse<double>(double.TryParse)
            .GetValueOrDefault(response.ExpiresIn);

        var tokens = TokenDecisions.CreateAuthTokens(
            response.AccessToken,
            response.RefreshToken,
            response.ExpiresIn,
            refreshExpiresIn,
            timeProvider.GetUtcNow());

        return Result<AuthorityClientError, AuthTokens>.Success(tokens);
    }

    /// <summary>
    /// Revokes a refresh token by calling the authority service.
    /// </summary>
    private async Task<Result<AuthorityClientError, Unit>> RevokeToken(string refreshToken)
    {
        var response = await GetClient().RevokeTokenAsync(new TokenRevocationRequest
        {
            Address = "/revocation",
            Token = refreshToken,
            TokenTypeHint = OidcConstants.TokenTypes.RefreshToken
        });

        if (response.IsError)
        {
            return Result<AuthorityClientError, Unit>.Failure(
                new AuthorityClientError.TokenRevocationFailed($"Unable to revoke refresh token from Organisation Authority: {response.Error}"));
        }

        return Result<AuthorityClientError, Unit>.Success(Unit.Value);
    }

    private HttpClient GetClient()
    {
        return httpClientFactory.CreateClient(OrganisationAuthorityHttpClientName);
    }
}

