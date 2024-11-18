using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CO.CDP.OrganisationApp.Authentication;

public class OidcEvents(IConfiguration configuration) : OpenIdConnectEvents
{
    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<OidcEvents>>();

        logger.LogError(context.Exception, "Oidc Authentication failed");

        if (context.Exception is InvalidOperationException invalidOpException &&
            invalidOpException.Message.Contains("Correlation failed"))
        {
            logger.LogError("Correlation failed. State: {State}", context.ProtocolMessage?.State);
            logger.LogError("Redirect URI: {RedirectUri}", context.ProtocolMessage?.RedirectUri);
            logger.LogError("Cookies: {Cookies}", context.HttpContext.Request.Headers["Cookie"].ToString());
            logger.LogError("Query: {Query}", context.HttpContext.Request.QueryString);
        }

        return base.AuthenticationFailed(context);
    }

    public override Task RemoteFailure(RemoteFailureContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<OidcEvents>>();

        logger.LogError(context.Failure, "Oidc Remote Failure, Redirect URI: {RedirectUri}",
            context.Request.Path + context.Request.QueryString);
        logger.LogError("Cookies: {Cookies}", context.HttpContext.Request.Headers["Cookie"].ToString());

        context.Response.Redirect($"/?one-login-error={Uri.EscapeDataString(context.Failure?.Message ?? "Unknown error")}");
        context.HandleResponse();

        return Task.CompletedTask;
    }

    public override Task RedirectToIdentityProvider(RedirectContext context)
    {
        context.ProtocolMessage.Parameters.Add("vtr", "[\"Cl.Cm\"]");
        context.ProtocolMessage.Parameters.Add("ui_locales", "en");

        return Task.CompletedTask;
    }

    public override Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
    {
        context.TokenEndpointRequest!.ClientAssertionType = OidcConstants.ClientAssertionTypes.JwtBearer;
        context.TokenEndpointRequest.ClientAssertion = CreateClientToken();

        return Task.CompletedTask;
    }

    private string CreateClientToken()
    {
        var clientId = configuration.GetValue<string>("OneLogin:ClientId")
            ?? throw new Exception("Missing configuration key: OneLogin:ClientId.");
        var authority = configuration.GetValue<string>("OneLogin:Authority")
            ?? throw new Exception("Missing configuration key: OneLogin:Authority.");
        var privateKey = configuration.GetValue<string>("OneLogin:PrivateKey")
            ?? throw new Exception("Missing configuration key: OneLogin:PrivateKey.");

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
}