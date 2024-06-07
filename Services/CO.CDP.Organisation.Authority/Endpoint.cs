using IdentityModel;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using static IdentityModel.OidcConstants;

namespace CO.CDP.Organisation.Authority;

public static class EndpointExtensions
{
    public static void UseIdentity(this WebApplication app, string issuer,
        RsaSecurityKey rsaPrivateKey, RSAParameters resPublicParams,
        OpenIdConnectConfiguration oneLoginConfiguration)
    {
        app.MapGet($"/{Discovery.DiscoveryEndpoint}", () => new
        {
            issuer,
            token_endpoint = $"{issuer}/token",
            jwks_uri = $"{issuer}/{Discovery.DiscoveryEndpoint}/jwks",
            response_types_supported = new[] { ResponseTypes.Token },
            scopes_supported = new[] { StandardScopes.OpenId },
            token_endpoint_auth_methods_supported = new[] { EndpointAuthenticationMethods.PostBody },
            token_endpoint_auth_signing_alg_values_supported = new[] { SecurityAlgorithms.RsaSha256 },
            grant_types_supported = new[] { GrantTypes.ClientCredentials },
            subject_types_supported = new[] { "public" },
            claim_types_supported = new[] { "normal" },
            claims_supported = new[] { JwtClaimTypes.Subject } // TODO: Add additional claims here <----
        });

        app.MapGet($"/{Discovery.DiscoveryEndpoint}/jwks", () => new
        {
            keys = new[] {
                new {
                    kty = "RSA",
                    use = "sig",
                    kid = "c2c3b22ac07f425eb893123de395464e",
                    alg = SecurityAlgorithms.RsaSha256,
                    n = Base64UrlEncoder.Encode(resPublicParams.Modulus!),
                    e = Base64UrlEncoder.Encode(resPublicParams.Exponent!)
                }
            }
        });

        app.MapPost("/token", async (HttpRequest request) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest("Invalid form content type");
            }

            var form = await request.ReadFormAsync();
            var grantType = form[TokenRequest.GrantType];
            var clientSecret = form[TokenRequest.ClientSecret];

            // Validate client credentials
            if (grantType != GrantTypes.ClientCredentials
                || !ValidToken(app.Logger, clientSecret, oneLoginConfiguration, out var urn))
            {
                return Results.BadRequest("Invalid client credentials");
            }

            // TODO: Fetch additional claims for authorisation purpose

            var tokenExpiry = 3600d;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("sub", urn!),
                    // TODO: Add additional claims here <----
                }),
                Expires = DateTime.UtcNow.AddSeconds(tokenExpiry),
                Issuer = issuer,
                SigningCredentials = new SigningCredentials(rsaPrivateKey, SecurityAlgorithms.RsaSha256)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Results.Ok(new
            {
                token_type = TokenResponse.BearerTokenType,
                access_token = tokenString,
                expires_in = tokenExpiry,
            });
        });
    }

    private static bool ValidToken(ILogger logger, string? token, OpenIdConnectConfiguration oneLoginConfiguration, out string? urn)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(token);

            var tokenHandler = new JwtSecurityTokenHandler();

            var jsonToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

            var issuerSigningPublicKey = oneLoginConfiguration.SigningKeys.FirstOrDefault(sk => sk.IsSupportedAlgorithm(jsonToken.SignatureAlgorithm))
                            ?? throw new Exception($"Missing {jsonToken.SignatureAlgorithm} auth signing security key.");

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = oneLoginConfiguration.Issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = issuerSigningPublicKey
            };

            tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);

            var sub = ((JwtSecurityToken)validatedToken).Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject)
                            ?? throw new Exception($"Missing 'sub' claim from JWT token.");

            urn = sub.Value;
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.ToString());
            urn = null;
            return false;
        }
    }
}
