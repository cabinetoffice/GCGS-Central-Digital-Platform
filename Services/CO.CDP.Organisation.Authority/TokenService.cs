using CO.CDP.OrganisationInformation.Persistence;
using IdentityModel;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CO.CDP.Organisation.Authority;

/// <summary>Structured log event definitions for authentication operations (NCSC Cloud Security Principle 13).</summary>
internal static class AuthEvents
{
    private static readonly Action<ILogger, string, string, string, bool, string?, Exception?> _authEvent =
        LoggerMessage.Define<string, string, string, bool, string?>(
            LogLevel.Information,
            new EventId(1000, "AuthEvent"),
            "AuthEvent={Event} Urn={Urn} GrantType={GrantType} Success={Success} Error={Error}");

    public static void Log(ILogger logger, string @event, string urn, string grantType, bool success, string? error = null, Exception? ex = null)
        => _authEvent(logger, @event, urn, grantType, success, error, ex);
}

public class TokenService(
    ILogger<TokenService> logger,
    IConfigurationService configService,
    IPersonRepository personRepository,
    IAuthorityRepository authorityRepository,
    IHttpClientFactory httpClientFactory,
    IOptions<FeaturesOptions> features) : ITokenService
{
    public async Task<Model.TokenResponse> CreateToken(string urn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(urn);

        if (!features.Value.ClaimsApiEnabled)
            logger.LogWarning(
                "ClaimsApiEnabled is FALSE — application roles will NOT be included in issued tokens. " +
                "Set Features:ClaimsApiEnabled=true in configuration to enable AppRegistry claims.");

        var authorityConfig    = configService.GetAuthorityConfiguration();
        var accessTokenExpiry  = authorityConfig.AccessTokenExpirySeconds;  // default 3600s
        var refreshTokenExpiry = authorityConfig.RefreshTokenExpirySeconds; // default 86400s
        string accessToken = await CreateAccessToken(urn, accessTokenExpiry);
        string refreshToken = await CreateRefreshToken(urn, refreshTokenExpiry);

        AuthEvents.Log(logger, "TokenIssued", urn, "client_credentials", true);

        return new Model.TokenResponse
        {
            TokenType = OidcConstants.TokenResponse.BearerTokenType,
            AccessToken = accessToken,
            ExpiresIn = accessTokenExpiry,
            RefreshToken = refreshToken,
            RefreshTokenExpiresIn = refreshTokenExpiry
        };
    }

    public async Task<(bool valid, string? urn)> ValidateOneLoginToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogInformation("Invalid onelogin token request, empty token");
            AuthEvents.Log(logger, "OneLoginValidationFailed", "", "client_credentials", false, "empty_token");
            return (false, null);
        }

        try
        {
            try
            {
                return await ValidateInternalAsync(token, refresh: false);
            }
            catch (SecurityTokenSignatureKeyNotFoundException ex)
            {
                logger.LogWarning(ex,
                    "JWT validation failed due to missing signing key. Refreshing JWKS and retrying once.");

                // Retry ONCE with refreshed JWKS
                return await ValidateInternalAsync(token, refresh: true);
            }
        }
        catch (Exception ex)
        {
            AuthEvents.Log(logger, "OneLoginValidationFailed", "", "client_credentials", false, ex.GetType().Name, ex);
            logger.LogError(ex, ex.Message);
            return (false, null);
        }
    }

    private async Task<(bool valid, string? urn)> ValidateInternalAsync(string token, bool refresh)
    {
        var oidcConfig      = await configService.GetOneLoginConfiguration(refresh);
        var authorityConfig = configService.GetAuthorityConfiguration();

        // Validate audience when OneLogin:ClientId is configured (OIDC Core 1.0 §3.1.3.7).
        // When the ClientId is absent (e.g. local dev without One Login), validation is skipped
        // with a warning rather than silently accepting all tokens.
        var validateAudience = !string.IsNullOrWhiteSpace(authorityConfig.OneLoginClientId);
        if (!validateAudience)
            logger.LogWarning(
                "OneLogin:ClientId is not configured — audience validation on incoming tokens is DISABLED. " +
                "Set OneLogin:ClientId to enforce audience checks.");

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = oidcConfig.Issuer,
            ValidateAudience         = validateAudience,
            ValidAudience            = authorityConfig.OneLoginClientId,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys        = oidcConfig.SigningKeys,
            ClockSkew                = TimeSpan.FromMinutes(2)
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);

        var jwt = (JwtSecurityToken)validatedToken;

        var sub = jwt.Subject
            ?? throw new Exception("Missing 'sub' claim from JWT token.");

        return (true, sub);
    }

    public async Task<(bool valid, string? urn)> ValidateAndRevokeRefreshToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogInformation("Invalid refresh token request, empty token");
            return (false, null);
        }

        try
        {
            var splits = token.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length != 2) return (false, null);
            var password      = splits[0];
            var salt          = Convert.FromBase64String(splits[1]);
            var hashToValidate = GenerateHash(password, salt);

            var storedToken = await authorityRepository.Find(hashToValidate);

            if (storedToken != null)
            {
                storedToken.Revoked = true;
                await authorityRepository.Save(storedToken);
                // Return the URN from the database record — not from the token string.
                AuthEvents.Log(logger, "RefreshTokenRevoked", storedToken.UserUrn ?? "", "refresh_token", true);
                return (true, storedToken.UserUrn);
            }

            AuthEvents.Log(logger, "RefreshTokenInvalid", "", "refresh_token", false, "token_not_found_or_expired");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.ToString());
        }
        return (false, null);
    }

    private async Task<string> CreateAccessToken(string urn, double tokenExpiry)
    {
        var config = configService.GetAuthorityConfiguration();

        List<Claim> claims = [
            new Claim(JwtClaimTypes.Subject, urn),
            new Claim("channel", "one-login")
        ];

        var person = await personRepository.FindByUrn(urn);
        claims.Add(new Claim(JwtClaimTypes.Roles, string.Join(",", person?.Scopes ?? [])));

        if (features.Value.ClaimsApiEnabled)
        {
            logger.LogDebug("Claims enrichment enabled for {UserUrn}. Fetching claims from Organisation API.", urn);
            try
            {
                var httpClient = httpClientFactory.CreateClient("OrganisationApiHttpClient");
                var encodedUrn = Uri.EscapeDataString(urn);
                using var response = await httpClient.GetAsync($"/organisations/claims/users/{encodedUrn}");

                if (response.IsSuccessStatusCode)
                {
                    var claimsJson = await response.Content.ReadAsStringAsync();
                    claims.Add(new Claim("cdp_claims", claimsJson, JsonClaimValueTypes.Json));
                    logger.LogDebug("Added cdp_claims for {UserUrn}.", urn);
                }
                else
                {
                    logger.LogWarning("Organisation API returned {StatusCode} for claims request for {UserUrn}.",
                        response.StatusCode, urn);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch user claims from Organisation API.");
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject            = new ClaimsIdentity(claims),
            Expires            = DateTime.UtcNow.AddSeconds(tokenExpiry),
            Issuer             = config.Issuer,
            Audience           = config.Issuer,   // aud = issuer (OWASP ASVS §3.5.1 — aud MUST be validated)
            SigningCredentials = new SigningCredentials(config.RsaPrivateKey, SecurityAlgorithms.RsaSha256),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        token.Header["kid"] = config.Kid;

        var tokenString = tokenHandler.WriteToken(token);
        return tokenString;
    }

    private async Task<string> CreateRefreshToken(string urn, double tokenExpiry)
    {
        var password  = GenerateRandomBase64String();
        // Use a cryptographically random salt — NOT the URN.
        // The URN is stored separately in the database record.
        // This ensures the opaque token string reveals no user identity (RFC 6749 §10.10).
        var saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
            rng.GetBytes(saltBytes);
        var saltB64   = Convert.ToBase64String(saltBytes);
        var tokenHash = GenerateHash(password, saltBytes);

        await authorityRepository.Save(new RefreshToken
        {
            TokenHash  = tokenHash,
            UserUrn    = urn,       // URN stored in DB — not in the token string
            Salt       = saltB64,   // Salt stored so validation can re-derive the hash
            ExpiryDate = DateTime.UtcNow.AddSeconds(tokenExpiry)
        });

        return $"{password}:{saltB64}";
    }

    private static string GenerateHash(string password, byte[] salt)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 100000, 256 / 8));
    }

    private static string GenerateRandomBase64String()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
