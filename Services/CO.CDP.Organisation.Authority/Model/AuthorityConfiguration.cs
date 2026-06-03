using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace CO.CDP.Organisation.Authority.Model;

public record AuthorityConfiguration
{
    public required string Issuer { get; init; }

    public required RsaSecurityKey RsaPrivateKey { get; init; }

    public required RSAParameters RsaPublicParams { get; init; }

    /// <summary>
    /// The OneLogin client ID registered for this Authority service.
    /// Used to validate the 'aud' claim on incoming OneLogin tokens (OIDC Core 1.0 §3.1.3.7).
    /// </summary>
    public string OneLoginClientId { get; init; } = string.Empty;

    /// <summary>
    /// RFC 7638 JWK Thumbprint derived from the RSA public key material by ConfigurationService.
    /// Changes automatically when the private key is rotated — no code change required.
    /// </summary>
    public string DerivedKid { get; init; } = string.Empty;

    /// <summary>
    /// Key identifier used in JWT headers and JWKS responses.
    /// Returns the RFC 7638-derived thumbprint when available;
    /// falls back to the legacy constant for local dev without a real private key.
    /// </summary>
    public string Kid => string.IsNullOrWhiteSpace(DerivedKid)
        ? "c2c3b22ac07f425eb893123de395464e"
        : DerivedKid;

    /// <summary>Access token lifetime in seconds. Defaults to 3600. Set via TokenExpiry:AccessTokenSeconds.</summary>
    public double AccessTokenExpirySeconds { get; init; } = 3600d;

    /// <summary>Refresh token lifetime in seconds. Defaults to 86400. Set via TokenExpiry:RefreshTokenSeconds.</summary>
    public double RefreshTokenExpirySeconds { get; init; } = 86400d;
}