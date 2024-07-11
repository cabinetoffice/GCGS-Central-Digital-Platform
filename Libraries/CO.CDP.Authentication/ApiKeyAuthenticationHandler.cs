using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CO.CDP.Authentication;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiKeyValidator apiKeyValidator)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    public const string ApiKeyHeaderName = "cdp-api-key";
    public const string AuthenticationScheme = "ApiKey";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

        if (apiKeyHeaderValues.Count == 0 || string.IsNullOrEmpty(providedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var (valid, organisation, scopes) = await apiKeyValidator.Validate(providedApiKey!);
        if (valid)
        {
            List<Claim> claims = [new Claim("channel", organisation == null ? "service-key" : "organisation-key")];
            if (organisation.HasValue)
            {
                claims.Add(new Claim("org", organisation.Value.ToString()));
            }
            if (scopes.Count > 0)
            {
                claims.Add(new Claim("scope", string.Join(" ", scopes)));
            }
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.Fail("Invalid API Key provided.");
    }
}

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
}