using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using static IdentityModel.OidcConstants;

namespace CO.CDP.OrganisationApp.Authentication;

public class OneLoginAuthority(
    IConfiguration config,
    ILogger<OneLoginAuthority> logger) : IOneLoginAuthority
{
    private OpenIdConnectConfiguration? _oneLoginConfig;

    public async Task<string?> ValidateLogoutToken(string logoutToken)
    {
        JwtSecurityToken jwtSecurityToken;
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
                IssuerSigningKeys = oneLoginConfig.SigningKeys,

                // Uncomment below line to override signingkey validation for testing purpose only
                //RequireSignedTokens = false,
                //SignatureValidator = (string token, TokenValidationParameters parameters) => new JwtSecurityToken(token)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(logoutToken, parameters, out var token);
            jwtSecurityToken = (JwtSecurityToken)token;
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "One Login logout token validation failed. {Token}", logoutToken);

            return null;
        }

        var urn = jwtSecurityToken.Subject;
        if (string.IsNullOrWhiteSpace(urn)) return null;

        var nonce = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "nonce")?.Value;
        if (!string.IsNullOrWhiteSpace(nonce)) return null;

        var eventsJson = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "events")?.Value;
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
