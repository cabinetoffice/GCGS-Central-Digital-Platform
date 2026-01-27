using CO.CDP.OrganisationInformation.Persistence;
using IdentityModel;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CO.CDP.Organisation.Authority;

public class TokenService(
    ILogger<TokenService> logger,
    IConfigurationService configService,
    IPersonRepository personRepository,
    IAuthorityRepository authorityRepository) : ITokenService
{
    public async Task<Model.TokenResponse> CreateToken(string urn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(urn);

        var accessTokenExpiry = 3600d; // 1 hour
        var refreshTokenExpiry = 86400d; // 1 day
        string accessToken = await CreateAccessToken(urn, accessTokenExpiry);
        string refreshToken = await CreateRefreshToken(urn, refreshTokenExpiry);

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
            logger.LogError(ex, ex.Message);
            return (false, null);
        }
    }

    private async Task<(bool valid, string? urn)> ValidateInternalAsync(string token, bool refresh)
    {
        var config = await configService.GetOneLoginConfiguration(refresh);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = config.Issuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = config.SigningKeys,
            ClockSkew = TimeSpan.FromMinutes(2)
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
            var password = splits[0];
            var salt = Convert.FromBase64String(splits[1]);
            var hashToValidate = GenerateHash(password, salt);

            var storedToken = await authorityRepository.Find(hashToValidate);

            if (storedToken != null)
            {
                storedToken.Revoked = true;
                await authorityRepository.Save(storedToken);
                return (true, Encoding.UTF8.GetString(salt));
            }
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

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddSeconds(tokenExpiry),
            Issuer = config.Issuer,
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
        var password = GenerateRandomBase64String();
        var salt = Encoding.UTF8.GetBytes(urn);
        var tokenHash = GenerateHash(password, salt);

        await authorityRepository.Save(new RefreshToken
        {
            TokenHash = tokenHash,
            ExpiryDate = DateTime.UtcNow.AddSeconds(tokenExpiry)
        });

        return $"{password}:{Convert.ToBase64String(salt)}";
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