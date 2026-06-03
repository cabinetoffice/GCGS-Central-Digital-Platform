using CO.CDP.Organisation.Authority.Model;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using static IdentityModel.OidcConstants;

namespace CO.CDP.Organisation.Authority;

public class ConfigurationService(IConfiguration config) : IConfigurationService
{
    private AuthorityConfiguration? authorityConfig;
    private OpenIdConnectConfiguration? oneLoginConfig;

    public AuthorityConfiguration GetAuthorityConfiguration()
    {
        if (authorityConfig == null)
        {
            var issuer = config["Issuer"] ?? throw new Exception("Missing configuration key: Issuer.");
            var privateKey = config["PrivateKey"] ?? throw new Exception("Missing configuration key: PrivateKey.");

            var rsaPrivate = RSA.Create();
            rsaPrivate.ImportFromPem(privateKey);
            var rsaPrivateKey = new RsaSecurityKey(rsaPrivate);

            var rsaPublic = RSA.Create();
            rsaPublic.ImportFromPem(rsaPrivate.ExportRSAPublicKeyPem());
            var rsaPublicParams = rsaPublic.ExportParameters(false);

            // Derive key ID as RFC 7638 JWK Thumbprint (SHA-256 of canonical key JSON).
            // This ensures kid changes automatically when the RSA key is rotated, allowing
            // JWT consumers to distinguish key versions and refresh JWKS accordingly.
            var kid = ComputeJwkThumbprint(rsaPublicParams);

            authorityConfig = new AuthorityConfiguration
            {
                Issuer                    = issuer,
                RsaPrivateKey             = rsaPrivateKey,
                RsaPublicParams           = rsaPublicParams,
                DerivedKid                = kid,
                OneLoginClientId          = config["OneLogin:ClientId"] ?? string.Empty,
                AccessTokenExpirySeconds  = config.GetValue<double?>("TokenExpiry:AccessTokenSeconds") ?? 3600d,
                RefreshTokenExpirySeconds = config.GetValue<double?>("TokenExpiry:RefreshTokenSeconds") ?? 86400d,
                AllowedClientIds          = config.GetSection("AllowedClientIds").Get<List<string>>() ?? []
            };
        }

        return authorityConfig;
    }

    public async Task<OpenIdConnectConfiguration> GetOneLoginConfiguration(bool refresh = false)
    {
        if (oneLoginConfig == null || refresh == true)
        {
            var authority = config["OneLogin:Authority"]
                ?? throw new Exception("Missing configuration key: OneLogin:Authority.");

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        new Uri(new Uri(authority), Discovery.DiscoveryEndpoint).ToString(),
                        new OpenIdConnectConfigurationRetriever());

            oneLoginConfig = await configurationManager.GetConfigurationAsync();
        }

        return oneLoginConfig;
    }

    /// <summary>
    /// Computes the RFC 7638 JWK Thumbprint for an RSA public key.
    /// The thumbprint is the Base64Url-encoded SHA-256 hash of the canonical JSON:
    /// {"e":"&lt;Base64Url(Exponent)&gt;","kty":"RSA","n":"&lt;Base64Url(Modulus)&gt;"}
    /// Members MUST be in lexicographic order (e, kty, n) with no whitespace.
    /// </summary>
    private static string ComputeJwkThumbprint(RSAParameters p)
    {
        var n     = Base64UrlEncoder.Encode(p.Modulus!);
        var e     = Base64UrlEncoder.Encode(p.Exponent!);
        var json  = $"{{\"e\":\"{e}\",\"kty\":\"RSA\",\"n\":\"{n}\"}}";
        var hash  = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Base64UrlEncoder.Encode(hash);
    }
}