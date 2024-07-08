using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace CO.CDP.Organisation.Authority.Model;

public record AuthorityConfiguration
{
    public required string Issuer { get; init; }

    public required RsaSecurityKey RsaPrivateKey { get; init; }

    public required RSAParameters RsaPublicParams { get; init; }
}