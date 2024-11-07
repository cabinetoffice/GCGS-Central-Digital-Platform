using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static IdentityModel.OidcConstants;

namespace CO.CDP.OrganisationApp.Authentication;

public class OneLoginAuthority(IConfiguration config) : IOneLoginAuthority
{
    private OpenIdConnectConfiguration? _oneLoginConfig;

    public async Task<string?> ValidateLogoutToken(string logoutToken)
    {
        ClaimsPrincipal claims;
        try
        {
            var oneLoginConfig = await GetConfiguration();

            var clientId = config["OneLogin:ClientId"]
                    ?? throw new Exception("Missing configuration key: OneLogin:ClientId.");

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = oneLoginConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = clientId,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = oneLoginConfig.SigningKeys
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            claims = tokenHandler.ValidateToken(logoutToken, parameters, out var _);
        }
        catch
        {
            return null;
        }

        var urn = claims.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(urn)) return null;

        var nonce = claims.FindFirstValue("nonce");
        if (!string.IsNullOrWhiteSpace(nonce)) return null;

        var eventsJson = claims.FindFirst("events")?.Value;
        if (string.IsNullOrWhiteSpace(eventsJson)) return null;

        var events = JObject.Parse(eventsJson);
        var logoutEvent = events.GetValue("http://schemas.openid.net/event/backchannel-logout");
        if (logoutEvent == null) return null;

        return urn;
    }

    private async Task<OpenIdConnectConfiguration> GetConfiguration()
    {
        if (_oneLoginConfig == null)
        {
            var authority = config["OneLogin:Authority"]
                ?? throw new Exception("Missing configuration key: OneLogin:Authority.");

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        new Uri(new Uri(authority), Discovery.DiscoveryEndpoint).ToString(),
                        new OpenIdConnectConfigurationRetriever());

            _oneLoginConfig = await configurationManager.GetConfigurationAsync();
        }

        return _oneLoginConfig;
    }
}
