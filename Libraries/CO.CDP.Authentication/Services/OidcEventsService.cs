using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace CO.CDP.Authentication.Services;

public class OidcEventsService : OpenIdConnectEvents
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OidcEventsService> _logger;

    public OidcEventsService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<OidcEventsService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;

        OnTokenValidated = HandleTokenValidated;
        OnRedirectToIdentityProvider = HandleRedirectToIdentityProvider;
        OnRemoteFailure = HandleRemoteFailure;
        OnAuthorizationCodeReceived = HandleAuthorizationCodeReceived;
    }

    private async Task HandleTokenValidated(TokenValidatedContext context)
    {
        _logger.LogInformation("HandleTokenValidated starting...");

        using var scope = _serviceProvider.CreateScope();
        var tokenExchangeService = scope.ServiceProvider.GetRequiredService<ITokenExchangeService>();
        var sessionManager = scope.ServiceProvider.GetRequiredService<ISessionManager>();

        var oneLoginToken = context.TokenEndpointResponse?.AccessToken;
        if (string.IsNullOrEmpty(oneLoginToken))
        {
            context.Fail("No OneLogin access token received");
            return;
        }

        try
        {
            var authorityTokens = await tokenExchangeService.ExchangeOneLoginTokenAsync(oneLoginToken);

            await sessionManager.SetTokensAsync(context.HttpContext, authorityTokens);

            var claims = new List<Claim>
            {
                new("authority:access_token", authorityTokens.AccessToken),
                new("authority:expires_at", authorityTokens.AccessTokenExpiry.ToString("O"))
            };

            var identity = (ClaimsIdentity)context.Principal!.Identity!;
            identity.AddClaims(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange OneLogin token for Authority tokens");
            context.Fail("Token exchange failed");
        }
    }

    private static Task HandleRedirectToIdentityProvider(RedirectContext context)
    {
        if (context.Properties.Items.TryGetValue("organisation_id", out var orgId))
        {
            context.ProtocolMessage.SetParameter("organisation_id", orgId);
        }

        context.ProtocolMessage.Parameters.Add("vtr", "[\"Cl.Cm\"]");
        context.ProtocolMessage.Parameters.Add("ui_locales", "en");

        return Task.CompletedTask;
    }

    private Task HandleAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
    {
        context.TokenEndpointRequest!.ClientAssertionType = OidcConstants.ClientAssertionTypes.JwtBearer;
        context.TokenEndpointRequest.ClientAssertion = CreateClientToken();

        return Task.CompletedTask;
    }

    private string CreateClientToken()
    {
        var clientId = _configuration["OneLogin:ClientId"]
            ?? throw new InvalidOperationException("Missing configuration key: OneLogin:ClientId.");
        var authority = _configuration["OneLogin:Authority"]
            ?? throw new InvalidOperationException("Missing configuration key: OneLogin:Authority.");
        var privateKey = _configuration["OneLogin:PrivateKey"]
            ?? throw new InvalidOperationException("Missing configuration key: OneLogin:PrivateKey.");

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKey);
        var credential = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };

        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            clientId,
            $"{authority}/token",
            [
                new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString()),
                new Claim(JwtClaimTypes.Subject, clientId),
                new Claim(JwtClaimTypes.IssuedAt, now.ToEpochTime().ToString(), ClaimValueTypes.Integer64)
            ],
            now,
            now.AddMinutes(5),
            credential
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private Task HandleRemoteFailure(RemoteFailureContext context)
    {
        _logger.LogError(context.Failure, "Authentication failed");
        context.Response.Redirect("/error?message=authentication-failed");
        context.HandleResponse();
        return Task.CompletedTask;
    }
}
