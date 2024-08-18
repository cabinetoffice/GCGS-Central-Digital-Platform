using AutoMapper;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence;
using IdentityModel;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static CO.CDP.OrganisationInformation.Persistence.ITenantRepository;

namespace CO.CDP.Organisation.Authority;

public class TokenService(
    ILogger<TokenService> logger,
    IConfigurationService configService,
    IMapper mapper,
    ITenantRepository tenantRepository,
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
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);

            var tokenHandler = new JwtSecurityTokenHandler();

            var jsonToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

            var config = await configService.GetOneLoginConfiguration();

            var issuerSigningPublicKey = config.SigningKeys.FirstOrDefault(sk => sk.IsSupportedAlgorithm(jsonToken.SignatureAlgorithm))
                            ?? throw new Exception($"Missing {jsonToken.SignatureAlgorithm} auth signing security key.");

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = config.Issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = issuerSigningPublicKey
            };

            tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);

            var sub = ((JwtSecurityToken)validatedToken).Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject)
                            ?? throw new Exception($"Missing 'sub' claim from JWT token.");

            return (true, sub.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.ToString());
            return (false, null);
        }
    }

    public async Task<(bool valid, string? urn)> ValidateRefreshToken(string? token)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);

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

        var tenantLookup = await GetTenant(urn);
        if (tenantLookup != null)
        {
            var compressedTenant = Convert.ToBase64String(Compress(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tenantLookup))));
            claims.Add(new Claim("ten", compressedTenant));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddSeconds(tokenExpiry),
            Issuer = config.Issuer,
            SigningCredentials = new SigningCredentials(config.RsaPrivateKey, SecurityAlgorithms.RsaSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
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
            ExpiryDate = DateTime.Now.AddSeconds(tokenExpiry)
        });

        return $"{password}:{Convert.ToBase64String(salt)}";
    }

    private async Task<OrganisationInformation.TenantLookup?> GetTenant(string urn)
    {
        OrganisationInformation.TenantLookup? tenantLookup;
        try
        {
            tenantLookup = await tenantRepository.LookupTenant(urn)
                                        .AndThen(mapper.Map<OrganisationInformation.TenantLookup>);
        }
        catch (TenantRepositoryException.TenantNotFoundException)
        {
            return null;
        }

        if (tenantLookup == null) return null;

        var numberOfOrgsToKeep = 10;
        foreach (var tenant in tenantLookup.Tenants)
        {
            var tenantOrgCount = tenant.Organisations.Count;
            var numberOfOrgsToAdd = tenantOrgCount > numberOfOrgsToKeep ? numberOfOrgsToKeep : tenantOrgCount;

            if (numberOfOrgsToAdd < tenantOrgCount)
            {
                tenant.Organisations.RemoveRange(numberOfOrgsToAdd, tenantOrgCount - numberOfOrgsToAdd);
            }

            numberOfOrgsToKeep -= numberOfOrgsToAdd;
        }

        return tenantLookup;
    }

    private static byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal, false))
        {
            gzipStream.Write(data);
        }

        return compressedStream.ToArray();
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