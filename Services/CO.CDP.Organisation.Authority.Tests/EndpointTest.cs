using CO.CDP.Organisation.Authority.Model;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using static IdentityModel.OidcConstants;

namespace CO.CDP.Organisation.Authority.Tests;

public class EndpointTest
{
    private readonly string _issuer = "https://issuer.com";
    private readonly HttpClient _client;
    private readonly Mock<IConfigurationService> _configurationService = new();
    private readonly Mock<ITokenService> _tokenService = new();

    public EndpointTest()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(s =>
            {
                s.AddSingleton(_ => _configurationService.Object);
                s.AddScoped(_ => _tokenService.Object);
            });
        });
        SetupConfigurationService();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDiscoveryEndpoint_ReturnsExpectedConfiguration()
    {
        var config = new OpenIdConfiguration
        {
            Issuer = _issuer,
            TokenEndpoint = $"{_issuer}/token",
            JwksUri = $"{_issuer}/.well-known/openid-configuration/jwks",
            ResponseTypesSupported = ["token"],
            ScopesSupported = ["openid"],
            TokenEndpointAuthMethodsSupported = ["client_secret_post"],
            TokenEndpointAuthSigningAlgValuesSupported = ["RS256"],
            GrantTypesSupported = ["client_credentials"],
            SubjectTypesSupported = ["public"],
            ClaimTypesSupported = ["normal"],
            ClaimsSupported = ["sub", "ten"]
        };

        var returnedConfig = await _client.GetFromJsonAsync<OpenIdConfiguration>("/.well-known/openid-configuration");

        returnedConfig.Should().NotBeNull();
        returnedConfig.Should().BeEquivalentTo(config, o => o.ComparingByMembers<OpenIdConfiguration>());
    }

    [Fact]
    public async Task GetJwksEndpoint_ReturnsExpectedKeys()
    {
        var jwks = new Model.JsonWebKeySet
        {
            Keys = [new()
            {
                Kty = "RSA",
                Use = "sig",
                Kid = "c2c3b22ac07f425eb893123de395464e",
                Alg = SecurityAlgorithms.RsaSha256,
                N = "0ND-OiUI9B_q2EARaXJzb8HaBUduUtkvxj7Qyb-5yw8JZ2_TO3aA0La6km-TLkBla9hwG8-NWTpx2tq-42YQU3L2274GI7dfUS8hqpJ6141PmchaO_10dII5e8GtRnaHqY8vWz5I03nPM9gYAT5h7bAlltN0scqCUnmvRmu1A0E5y65OT63oQIUAo9PpVqckxM0yHgYKlf6mtTu3dkd8LuhciRnNGaJ5_OGEeyeZrzBzgvE2zmrH3H66Oq9dkvr5K_Unhd7Lo4EdGyaI31nRSRshY7F_wVSIgsebya9tGHalh1pgSzL0qr9v1-WU5gG_XKvpFuIeByhSWvjsZJykiw",
                E = "AQAB"
            }]
        };

        var returnedJwks = await _client.GetFromJsonAsync<Model.JsonWebKeySet>("/.well-known/openid-configuration/jwks");

        returnedJwks.Should().NotBeNull();
        returnedJwks.Should().BeEquivalentTo(jwks, o => o.ComparingByMembers<Model.JsonWebKeySet>());
    }

    [Fact]
    public async Task PostTokenEndpoint_ValidRequest_ShouldReturn200AndToken()
    {
        var clientSecret = "valid-secret";
        var urn = "urn:fdc:gov.uk:2022:6fTvD1cMhQNJxrLZSyBgo5";
        var formData = new Dictionary<string, string>
        {
            { "grant_type", GrantTypes.ClientCredentials },
            { "client_secret", clientSecret }
        };
        var tokenResponse = new Model.TokenResponse
        {
            AccessToken = "tempToken",
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };

        _tokenService.Setup(c => c.ValidateOneLoginToken(clientSecret)).ReturnsAsync((true, urn));
        _tokenService.Setup(c => c.CreateToken(urn)).ReturnsAsync(tokenResponse);

        var response = await _client.PostAsync("/token", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Model.TokenResponse>();
        content.Should().NotBeNull()
            .And.BeOfType<Model.TokenResponse>()
            .Which.Should().BeEquivalentTo(tokenResponse);
    }

    [Fact]
    public async Task PostTokenEndpoint_InvalidRequestType_ReturnsUnsupportedMediaTypeResponse()
    {
        var response = await _client.PostAsync("/token", new StringContent("test"));

        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task PostTokenEndpoint_InvalidClientSecret_ShouldReturn400()
    {
        var clientSecret = "invalid-secret";
        var formData = new Dictionary<string, string>
        {
            { "grant_type", GrantTypes.ClientCredentials },
            { "client_secret", clientSecret }
        };
        _tokenService.Setup(c => c.ValidateOneLoginToken(clientSecret)).ReturnsAsync((false, null));

        var response = await _client.PostAsync("/token", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull()
            .And.BeOfType<ProblemDetails>()
            .Which.Detail.Should().Be("Invalid client secret");
    }

    [Fact]
    public async Task PostTokenEndpoint_InvalidGrantType_ShouldReturn400()
    {
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "invalid-grant-type" },
            { "client_secret", "valid-secret" }
        };

        var response = await _client.PostAsync("/token", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull()
            .And.BeOfType<ProblemDetails>()
            .Which.Detail.Should().Be("Invalid grant type");
    }

    private void SetupConfigurationService()
    {
        var privateKey = "-----BEGIN RSA PRIVATE KEY-----\nMIIEowIBAAKCAQEA0ND+OiUI9B/q2EARaXJzb8HaBUduUtkvxj7Qyb+5yw8JZ2/T\nO3aA0La6km+TLkBla9hwG8+NWTpx2tq+42YQU3L2274GI7dfUS8hqpJ6141Pmcha\nO/10dII5e8GtRnaHqY8vWz5I03nPM9gYAT5h7bAlltN0scqCUnmvRmu1A0E5y65O\nT63oQIUAo9PpVqckxM0yHgYKlf6mtTu3dkd8LuhciRnNGaJ5/OGEeyeZrzBzgvE2\nzmrH3H66Oq9dkvr5K/Unhd7Lo4EdGyaI31nRSRshY7F/wVSIgsebya9tGHalh1pg\nSzL0qr9v1+WU5gG/XKvpFuIeByhSWvjsZJykiwIDAQABAoIBAAskMBMq+40sGaU5\njeH9bEzCDcic5Owp2YpGKJVmiqzJV1CuPyv7v6gHvroUBH/mibB+YETrmVQWKKQh\n/JSBwN8+ZA7i1s1OehkI8SoxZt3uOT3CkUqEr9xoG3p9WIWSFAGCudSLwFfXEhod\n5V+znRkjWZKnR6Feif73G/zWUg1svKeirhGZ+652c69lnddq4BVRRBqcgd7z9QDQ\n9aNygcQbj+3JTvXs3H642nCAd2j5NuXnF+QNK1gFg4jSuqz3XtGCELm87rXSpn/m\n1BUbcy7cQl/Le5MX2TGGsDzeKsdTQMCWhacHMEncN216373ceS4FDfoqYpyVi9ho\ntJTWFwkCgYEA8ilHOLwtAkinSVosmpS/h5cvXt724B17z/YaFlFojEunav1LDNnA\nHTNAfrC5jXvA/FSltDO0+S3KL4DH2qoaA1lfimNdjm35VWLP9EhtAc8dWN1BbT3a\nRrHH/WTO+IuyfgTy4K6M8wzU3yrV/B4xxRiciLKZ7GCztVwYSrKV3r8CgYEA3L/k\nlIPSbMKMPdc7rft4Q6sLUaEDcApUbQkD2fKFonqe6cVIQkbyTWGEfVnDnhcBJ9JE\nxNb61n3c1I/ByrNDehqaDVnmFNXLp7JXm0cFzoCG7upZx0soR+2ksl8YRLTnn/ma\nooZiAuuuqt28KURseDdonyY5bpXd5DZ9CH9DOTUCgYAlMMO4aeX7sM7ITCoHf4q+\ntzBWQKXnm3+VW7V78fq9eSz4GHh7O8Hueh2Ql3GX0ga8ef+M4lgL4MVpcDzBT1h5\nZTNwHHyU0Dz3qRpI6QqcuYNHT4upzFMGgm01dFL5BvNduULX0NbiyPi0YW7Frpl0\nLKh1sgBDtkJDOyab6jtsHwKBgFMoNoN75RjzcMEbA3BW+scC+BCYByN9wLASZVEE\n+zJp3tNRbhcJgt2DjtVpqzcyk0zc+Ort0TBb2YR5Yxdo0FJ/EulUpEfVAhL6K+Zi\nqt1PrYTy4z8gINx3uUM4b3vyag7piEcROHrBLdtQDEG/dN0UgmTxkVEHQ79kh1Vc\nG4bBAoGBANgDVFyGJslPgRuXbOwEOHBY5tHmvfY7Z1fF4VEBsAPn+UwtS4W8xuCg\nVQazBEmPaeJt6jx73pT7sGTocP9bUJ134CmI/k32Mu5dViTT6lY09DO2OdauColb\nUIM+sEIcV4mz42lu0A9sta3osaHItHZOGpIj9V1OA/2av/bNeYi6\n-----END RSA PRIVATE KEY-----";

        var rsaPrivate = RSA.Create();
        rsaPrivate.ImportFromPem(privateKey);
        var rsaPrivateKey = new RsaSecurityKey(rsaPrivate);

        var rsaPublic = RSA.Create();
        rsaPublic.ImportFromPem(rsaPrivate.ExportRSAPublicKeyPem());
        var resPublicParams = rsaPublic.ExportParameters(false);

        _configurationService.Setup(c => c.GetAuthorityConfiguration())
            .Returns(new AuthorityConfiguration { Issuer = _issuer, RsaPrivateKey = rsaPrivateKey, RsaPublicParams = resPublicParams });
    }
}
