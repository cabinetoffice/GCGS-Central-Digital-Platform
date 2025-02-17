using CO.CDP.Organisation.Authority.Model;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net.Mime;
using static IdentityModel.OidcConstants;

namespace CO.CDP.Organisation.Authority;

public static class EndpointExtensions
{
    public static void UseIdentity(this WebApplication app)
    {
        app.MapGet($"/{Discovery.DiscoveryEndpoint}",
            (IConfigurationService service) =>
            {
                var config = service.GetAuthorityConfiguration();
                return new OpenIdConfiguration
                {
                    Issuer = config.Issuer,
                    TokenEndpoint = $"{config.Issuer}/token",
                    RevocationEndpoint = $"{config.Issuer}/revocation",
                    JwksUri = $"{config.Issuer}/{Discovery.DiscoveryEndpoint}/jwks",
                    ResponseTypesSupported = [ResponseTypes.Token],
                    ScopesSupported = [StandardScopes.OpenId],
                    TokenEndpointAuthMethodsSupported = [EndpointAuthenticationMethods.PostBody],
                    TokenEndpointAuthSigningAlgValuesSupported = [SecurityAlgorithms.RsaSha256],
                    GrantTypesSupported = [GrantTypes.ClientCredentials, GrantTypes.RefreshToken],
                    SubjectTypesSupported = ["public"],
                    ClaimTypesSupported = ["normal"],
                    ClaimsSupported = [JwtClaimTypes.Subject, "channel", JwtClaimTypes.Roles]
                };
            })
            .Produces<OpenIdConfiguration>(StatusCodes.Status200OK);

        app.MapGet($"/{Discovery.DiscoveryEndpoint}/jwks",
            (IConfigurationService service) =>
            {
                var config = service.GetAuthorityConfiguration();
                return new Model.JsonWebKeySet
                {
                    Keys = [new()
                    {
                        Kty = "RSA",
                        Use = "sig",
                        Kid = config.Kid,
                        Alg = SecurityAlgorithms.RsaSha256,
                        N = Base64UrlEncoder.Encode(config.RsaPublicParams.Modulus!),
                        E = Base64UrlEncoder.Encode(config.RsaPublicParams.Exponent!)
                    }]
                };
            })
            .Produces<Model.JsonWebKeySet>(StatusCodes.Status200OK);

        app.MapPost("/token",
            async ([AsParameters] Model.TokenRequest request, ITokenService service) =>
            {
                switch (request.GrantType)
                {
                    case GrantTypes.ClientCredentials:
                        var (valid, urn) = await service.ValidateOneLoginToken(request.ClientSecret);
                        if (valid && !string.IsNullOrWhiteSpace(urn))
                        {
                            return Results.Ok(await service.CreateToken(urn));
                        }
                        return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Invalid client secret");

                    case GrantTypes.RefreshToken:
                        (valid, urn) = await service.ValidateAndRevokeRefreshToken(request.RefreshToken);
                        if (valid && !string.IsNullOrWhiteSpace(urn))
                        {
                            return Results.Ok(await service.CreateToken(urn));
                        }
                        return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Invalid refresh token");

                    default:
                        return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Invalid grant type");
                }
            })
            .DisableAntiforgery()
            .WithMetadata(new ConsumesAttribute(MediaTypeNames.Application.FormUrlEncoded))
            .Produces<Model.TokenResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        app.MapPost("/revocation",
            async ([AsParameters] RevocationRequest request, ITokenService service) =>
            {
                switch (request.TokenTypeHint)
                {
                    case TokenTypes.RefreshToken:
                        await service.ValidateAndRevokeRefreshToken(request.Token);
                        return Results.Ok();

                    default:
                        return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Unsupported token type");
                }
            })
            .DisableAntiforgery()
            .WithMetadata(new ConsumesAttribute(MediaTypeNames.Application.FormUrlEncoded))
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    public static void DocumentApi(this SwaggerGenOptions options, IConfigurationManager configuration)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = configuration.GetValue("Version", "dev"),
            Title = "Organisation Authority API",
            Description = "API for generating token using OpenId Connect Client Credentials flow."
        });
    }
}