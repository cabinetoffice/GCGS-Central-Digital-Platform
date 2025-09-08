using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CO.CDP.RegisterOfCommercialTools.App.Authentication;

public class OidcEvents(
    IConfiguration configuration,
    ILogger<OidcEvents> logger,
    IOptionsMonitor<CookieAuthenticationOptions> cookieAuthOptions) : OpenIdConnectEvents
{
    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        logger.LogError(context.Exception,
            "Oidc Authentication failed.{NewLine}State: {State}{NewLine}Redirect URI: {RedirectUri}{NewLine}Cookies: {Cookies}{NewLine}Query: {Query}",
            Environment.NewLine,
            SanitiseForLogging(context.ProtocolMessage.State),
            Environment.NewLine,
            SanitiseForLogging(context.ProtocolMessage.RedirectUri),
            Environment.NewLine,
            SanitiseForLogging(context.HttpContext.Request.Headers.Cookie.ToString()),
            Environment.NewLine,
            SanitiseForLogging(context.HttpContext.Request.QueryString.ToString()));

        return base.AuthenticationFailed(context);
    }

    public override async Task RemoteFailure(RemoteFailureContext context)
    {
        var authCookieName = cookieAuthOptions.Get(CookieAuthenticationDefaults.AuthenticationScheme).Cookie.Name ??
                             ".AspNetCore.Cookies";

        if (context.HttpContext.Request.Cookies.TryGetValue(authCookieName, out var authCookie) &&
            !string.IsNullOrEmpty(authCookie))
        {
            var authenticateResult = await context.HttpContext.AuthenticateAsync();

            if (authenticateResult.Succeeded)
            {
                context.Response.Redirect("/");
                context.HandleResponse();
                return;
            }
        }

        logger.LogError(context.Failure,
            "Oidc Remote Failure.{NewLine}Redirect URI: {RedirectUri}{NewLine}Cookies: {Cookies}",
            Environment.NewLine,
            SanitiseForLogging(context.Request.Path + context.Request.QueryString),
            Environment.NewLine,
            SanitiseForLogging(context.HttpContext.Request.Headers["Cookie"].ToString()));

        context.Response.Redirect(
            $"/?one-login-error={Uri.EscapeDataString(context.Failure?.Message ?? "Unknown error")}");
        context.HandleResponse();
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

    private static string SanitiseForLogging(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return "\"\"";

        var sanitised = new string(input.Where(c => !char.IsControl(c) && c != '\t').ToArray())
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace("\t", "");

        var output = sanitised.Length > 500 ? sanitised[..500] + "..." : sanitised;
        return $"\"{output}\"";
    }
}