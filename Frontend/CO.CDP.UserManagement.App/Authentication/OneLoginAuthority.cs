using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using static IdentityModel.OidcConstants;

namespace CO.CDP.UserManagement.App.Authentication;

public class OneLoginAuthority(
    IOptions<OneLoginOptions> oneLoginOptions,
    ILogger<OneLoginAuthority> logger) : IOneLoginAuthority
{
    private const string BackChannelLogoutEventClaim = "http://schemas.openid.net/event/backchannel-logout";
    private const string NonceClaim = "nonce";
    private const string EventsClaim = "events";
    private OpenIdConnectConfiguration? _oneLoginConfig;

    public async Task<string?> ValidateLogoutToken(string logoutToken)
    {
        var validatedToken = await TryValidateToken(logoutToken);
        return validatedToken is null ? null : ExtractUserUrn(validatedToken);
    }

    private async Task<OpenIdConnectConfiguration> GetConfiguration()
    {
        if (_oneLoginConfig == null)
        {
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                new Uri(new Uri(oneLoginOptions.Value.Authority), Discovery.DiscoveryEndpoint).ToString(),
                new OpenIdConnectConfigurationRetriever());

            _oneLoginConfig = await configurationManager.GetConfigurationAsync();
        }

        return _oneLoginConfig;
    }

    private async Task<JwtSecurityToken?> TryValidateToken(string logoutToken)
    {
        try
        {
            var oneLoginConfig = await GetConfiguration();
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(
                logoutToken,
                CreateTokenValidationParameters(oneLoginConfig, oneLoginOptions.Value.ClientId),
                out var token);

            return token as JwtSecurityToken;
        }
        catch (SecurityTokenException ex)
        {
            logger.LogInformation(ex, "One Login logout token validation failed.");
            return null;
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "One Login logout token validation failed.");
            return null;
        }
    }

    private static TokenValidationParameters CreateTokenValidationParameters(
        OpenIdConnectConfiguration oneLoginConfig,
        string clientId) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = oneLoginConfig.Issuer,
        ValidateAudience = true,
        ValidAudience = clientId,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKeys = oneLoginConfig.SigningKeys
    };

    private static string? ExtractUserUrn(JwtSecurityToken jwtSecurityToken)
    {
        var urn = jwtSecurityToken.Subject;
        if (string.IsNullOrWhiteSpace(urn))
        {
            return null;
        }

        var nonce = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == NonceClaim)?.Value;
        if (!string.IsNullOrWhiteSpace(nonce))
        {
            return null;
        }

        var eventsJson = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == EventsClaim)?.Value;
        if (string.IsNullOrWhiteSpace(eventsJson))
        {
            return null;
        }

        return HasBackChannelLogoutEvent(eventsJson) ? urn : null;
    }

    private static bool HasBackChannelLogoutEvent(string eventsJson)
    {
        try
        {
            using var events = JsonDocument.Parse(eventsJson);
            return events.RootElement.TryGetProperty(BackChannelLogoutEventClaim, out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
