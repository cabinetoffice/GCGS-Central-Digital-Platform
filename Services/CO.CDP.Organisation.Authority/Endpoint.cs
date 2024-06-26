using CO.CDP.Organisation.Authority.Model;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using static IdentityModel.OidcConstants;

namespace CO.CDP.Organisation.Authority;

public static class EndpointExtensions
{
    public static void UseIdentity(this WebApplication app, string issuer,
        RsaSecurityKey rsaPrivateKey, RSAParameters rsaPublicParams)
    {
        app.MapGet($"/{Discovery.DiscoveryEndpoint}",
            () => new OpenIdConfiguration
            {
                Issuer = issuer,
                TokenEndpoint = $"{issuer}/token",
                JwksUri = $"{issuer}/{Discovery.DiscoveryEndpoint}/jwks",
                ResponseTypesSupported = [ResponseTypes.Token],
                ScopesSupported = [StandardScopes.OpenId],
                TokenEndpointAuthMethodsSupported = [EndpointAuthenticationMethods.PostBody],
                TokenEndpointAuthSigningAlgValuesSupported = [SecurityAlgorithms.RsaSha256],
                GrantTypesSupported = [GrantTypes.ClientCredentials],
                SubjectTypesSupported = ["public"],
                ClaimTypesSupported = ["normal"],
                ClaimsSupported = [JwtClaimTypes.Subject] // TODO: Add additional claims here <----
            })
            .Produces<OpenIdConfiguration>(StatusCodes.Status200OK, "application/json");

        app.MapGet($"/{Discovery.DiscoveryEndpoint}/jwks",
            () => new Model.JsonWebKeySet
            {
                Keys = [new()
                {
                    Kty = "RSA",
                    Use = "sig",
                    Kid = "c2c3b22ac07f425eb893123de395464e",
                    Alg = SecurityAlgorithms.RsaSha256,
                    N = Base64UrlEncoder.Encode(rsaPublicParams.Modulus!),
                    E = Base64UrlEncoder.Encode(rsaPublicParams.Exponent!)
                }]
            })
            .Produces<Model.JsonWebKeySet>(StatusCodes.Status200OK, "application/json");

        app.MapPost("/token",
            async ([FromForm] string grant_type, [FromForm] string client_secret, IOpenIdConfiguration config) =>
            {
                var oneloginConfig = await config.Get();

                // Validate client credentials
                if (grant_type != GrantTypes.ClientCredentials
                        || !ValidToken(app.Logger, client_secret, oneloginConfig, out var urn))
                {
                    return Results.BadRequest("Invalid client credentials");
                }

                return Results.Ok(CreateToken(issuer, rsaPrivateKey, urn!));
            })
            .DisableAntiforgery()
            .WithMetadata(new ConsumesAttribute("application/x-www-form-urlencoded"))
            .Produces<Model.TokenResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status415UnsupportedMediaType); ;
    }

    private static Model.TokenResponse CreateToken(string issuer, RsaSecurityKey rsaPrivateKey, string urn)
    {
        // TODO: Fetch additional claims for authorisation purpose

        var tokenExpiry = 3600d;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                    new Claim("sub", urn),
                    // TODO: Add additional claims here <----
                }),
            Expires = DateTime.UtcNow.AddSeconds(tokenExpiry),
            Issuer = issuer,
            SigningCredentials = new SigningCredentials(rsaPrivateKey, SecurityAlgorithms.RsaSha256)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new Model.TokenResponse
        {
            TokenType = OidcConstants.TokenResponse.BearerTokenType,
            AccessToken = tokenString,
            ExpiresIn = tokenExpiry,
        };
    }

    private static bool ValidToken(ILogger logger, string? token, OpenIdConnectConfiguration oneLoginConfig, out string? urn)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(token);

            var tokenHandler = new JwtSecurityTokenHandler();

            var jsonToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

            var issuerSigningPublicKey = oneLoginConfig.SigningKeys.FirstOrDefault(sk => sk.IsSupportedAlgorithm(jsonToken.SignatureAlgorithm))
                            ?? throw new Exception($"Missing {jsonToken.SignatureAlgorithm} auth signing security key.");

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = oneLoginConfig.Issuer,
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

    public static void DocumentApi(this SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "1.0.0",
            Title = "Organisation Authority API",
            Description = "API for generating token using OpenId Connect Client Credentials flow.",
            TermsOfService = new Uri("https://example.com/terms"),
            Contact = new OpenApiContact
            {
                Name = "Example Contact",
                Url = new Uri("https://example.com/contact")
            },
            License = new OpenApiLicense
            {
                Name = "Example License",
                Url = new Uri("https://example.com/license")
            }
        });
    }
}