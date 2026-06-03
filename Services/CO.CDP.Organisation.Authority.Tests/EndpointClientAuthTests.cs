using CO.CDP.Organisation.Authority.Model;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using static IdentityModel.OidcConstants;

namespace CO.CDP.Organisation.Authority.Tests;

public class EndpointClientAuthTests
{
    private readonly string _issuer = "https://issuer.com";

    private HttpClient CreateClient(
        IReadOnlyList<string> allowedClientIds,
        Mock<ITokenService> tokenServiceMock)
    {
        var configServiceMock = new Mock<IConfigurationService>();

        var privateKey = "-----BEGIN RSA PRIVATE KEY-----\nMIIEowIBAAKCAQEA0ND+OiUI9B/q2EARaXJzb8HaBUduUtkvxj7Qyb+5yw8JZ2/T\nO3aA0La6km+TLkBla9hwG8+NWTpx2tq+42YQU3L2274GI7dfUS8hqpJ6141Pmcha\nO/10dII5e8GtRnaHqY8vWz5I03nPM9gYAT5h7bAlltN0scqCUnmvRmu1A0E5y65O\nT63oQIUAo9PpVqckxM0yHgYKlf6mtTu3dkd8LuhciRnNGaJ5/OGEeyeZrzBzgvE2\nzmrH3H66Oq9dkvr5K/Unhd7Lo4EdGyaI31nRSRshY7F/wVSIgsebya9tGHalh1pg\nSzL0qr9v1+WU5gG/XKvpFuIeByhSWvjsZJykiwIDAQABAoIBAAskMBMq+40sGaU5\njeH9bEzCDcic5Owp2YpGKJVmiqzJV1CuPyv7v6gHvroUBH/mibB+YETrmVQWKKQh\n/JSBwN8+ZA7i1s1OehkI8SoxZt3uOT3CkUqEr9xoG3p9WIWSFAGCudSLwFfXEhod\n5V+znRkjWZKnR6Feif73G/zWUg1svKeirhGZ+652c69lnddq4BVRRBqcgd7z9QDQ\n9aNygcQbj+3JTvXs3H642nCAd2j5NuXnF+QNK1gFg4jSuqz3XtGCELm87rXSpn/m\n1BUbcy7cQl/Le5MX2TGGsDzeKsdTQMCWhacHMEncN216373ceS4FDfoqYpyVi9ho\ntJTWFwkCgYEA8ilHOLwtAkinSVosmpS/h5cvXt724B17z/YaFlFojEunav1LDNnA\nHTNAfrC5jXvA/FSltDO0+S3KL4DH2qoaA1lfimNdjm35VWLP9EhtAc8dWN1BbT3a\nRrHH/WTO+IuyfgTy4K6M8wzU3yrV/B4xxRiciLKZ7GCztVwYSrKV3r8CgYEA3L/k\nlIPSbMKMPdc7rft4Q6sLUaEDcApUbQkD2fKFonqe6cVIQkbyTWGEfVnDnhcBJ9JE\nxNb61n3c1I/ByrNDehqaDVnmFNXLp7JXm0cFzoCG7upZx0soR+2ksl8YRLTnn/ma\nooZiAuuuqt28KURseDdonyY5bpXd5DZ9CH9DOTUCgYAlMMO4aeX7sM7ITCoHf4q+\ntzBWQKXnm3+VW7V78fq9eSz4GHh7O8Hueh2Ql3GX0ga8ef+M4lgL4MVpcDzBT1h5\nZTNwHHyU0Dz3qRpI6QqcuYNHT4upzFMGgm01dFL5BvNduULX0NbiyPi0YW7Frpl0\nLKh1sgBDtkJDOyab6jtsHwKBgFMoNoN75RjzcMEbA3BW+scC+BCYByN9wLASZVEE\n+zJp3tNRbhcJgt2DjtVpqzcyk0zc+Ort0TBb2YR5Yxdo0FJ/EulUpEfVAhL6K+Zi\nqt1PrYTy4z8gINx3uUM4b3vyag7piEcROHrBLdtQDEG/dN0UgmTxkVEHQ79kh1Vc\nG4bBAoGBANgDVFyGJslPgRuXbOwEOHBY5tHmvfY7Z1fF4VEBsAPn+UwtS4W8xuCg\nVQazBEmPaeJt6jx73pT7sGTocP9bUJ134CmI/k32Mu5dViTT6lY09DO2OdauColb\nUIM+sEIcV4mz42lu0A9sta3osaHItHZOGpIj9V1OA/2av/bNeYi6\n-----END RSA PRIVATE KEY-----";

        var rsaPrivate = RSA.Create();
        rsaPrivate.ImportFromPem(privateKey);
        var rsaPrivateKey = new RsaSecurityKey(rsaPrivate);

        var rsaPublic = RSA.Create();
        rsaPublic.ImportFromPem(rsaPrivate.ExportRSAPublicKeyPem());
        var rsaPublicParams = rsaPublic.ExportParameters(false);

        configServiceMock.Setup(c => c.GetAuthorityConfiguration())
            .Returns(new AuthorityConfiguration
            {
                Issuer = _issuer,
                RsaPrivateKey = rsaPrivateKey,
                RsaPublicParams = rsaPublicParams,
                AllowedClientIds = allowedClientIds
            });

        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(s =>
            {
                s.AddSingleton(_ => configServiceMock.Object);
                s.AddScoped(_ => tokenServiceMock.Object);
            });
        });

        return factory.CreateClient();
    }

    [Fact]
    public async Task PostTokenEndpoint_UnknownClientId_Returns401()
    {
        var tokenServiceMock = new Mock<ITokenService>();
        var client = CreateClient(["organisation-app", "commercial-tools-app"], tokenServiceMock);

        var formData = new Dictionary<string, string>
        {
            { "grant_type", GrantTypes.ClientCredentials },
            { "client_id", "unknown-app" },
            { "client_secret", "some-secret" }
        };

        var response = await client.PostAsync("/token", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("error").GetString().Should().Be("invalid_client");
        body.GetProperty("error_description").GetString().Should().Be("client_id is required");

        tokenServiceMock.Verify(s => s.ValidateOneLoginToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PostTokenEndpoint_MissingClientId_WithAllowedClientIdsConfigured_Returns401()
    {
        var tokenServiceMock = new Mock<ITokenService>();
        var client = CreateClient(["organisation-app", "commercial-tools-app"], tokenServiceMock);

        var formData = new Dictionary<string, string>
        {
            { "grant_type", GrantTypes.ClientCredentials },
            { "client_secret", "some-secret" }
        };

        var response = await client.PostAsync("/token", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("error").GetString().Should().Be("invalid_client");
        body.GetProperty("error_description").GetString().Should().Be("client_id is required");

        tokenServiceMock.Verify(s => s.ValidateOneLoginToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PostTokenEndpoint_KnownClientId_WithValidOneLoginToken_Returns200()
    {
        var urn = "urn:fdc:gov.uk:2022:6fTvD1cMhQNJxrLZSyBgo5";
        var clientSecret = "valid-secret";
        var tokenResponse = new Model.TokenResponse
        {
            AccessToken = "tempToken",
            ExpiresIn = 3600,
            TokenType = "Bearer",
            RefreshToken = "refreshToken",
            RefreshTokenExpiresIn = 36000
        };

        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock.Setup(s => s.ValidateOneLoginToken(clientSecret)).ReturnsAsync((true, urn));
        tokenServiceMock.Setup(s => s.CreateToken(urn)).ReturnsAsync(tokenResponse);

        var client = CreateClient(["organisation-app", "commercial-tools-app"], tokenServiceMock);

        var formData = new Dictionary<string, string>
        {
            { "grant_type", GrantTypes.ClientCredentials },
            { "client_id", "organisation-app" },
            { "client_secret", clientSecret }
        };

        var response = await client.PostAsync("/token", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Model.TokenResponse>();
        content.Should().NotBeNull()
            .And.BeOfType<Model.TokenResponse>()
            .Which.Should().BeEquivalentTo(tokenResponse);
    }

    [Fact]
    public async Task PostTokenEndpoint_KnownClientId_WithInvalidOneLoginToken_Returns400()
    {
        var clientSecret = "invalid-secret";
        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock.Setup(s => s.ValidateOneLoginToken(clientSecret)).ReturnsAsync((false, null));

        var client = CreateClient(["organisation-app", "commercial-tools-app"], tokenServiceMock);

        var formData = new Dictionary<string, string>
        {
            { "grant_type", GrantTypes.ClientCredentials },
            { "client_id", "organisation-app" },
            { "client_secret", clientSecret }
        };

        var response = await client.PostAsync("/token", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostTokenEndpoint_EmptyAllowedClientIds_AcceptsRequestWithoutClientId()
    {
        var urn = "urn:fdc:gov.uk:2022:6fTvD1cMhQNJxrLZSyBgo5";
        var clientSecret = "valid-secret";
        var tokenResponse = new Model.TokenResponse
        {
            AccessToken = "tempToken",
            ExpiresIn = 3600,
            TokenType = "Bearer",
            RefreshToken = "refreshToken",
            RefreshTokenExpiresIn = 36000
        };

        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock.Setup(s => s.ValidateOneLoginToken(clientSecret)).ReturnsAsync((true, urn));
        tokenServiceMock.Setup(s => s.CreateToken(urn)).ReturnsAsync(tokenResponse);

        // Empty allow-list = validation disabled (backwards compatibility)
        var client = CreateClient([], tokenServiceMock);

        var formData = new Dictionary<string, string>
        {
            { "grant_type", GrantTypes.ClientCredentials },
            { "client_secret", clientSecret }
        };

        var response = await client.PostAsync("/token", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostTokenEndpoint_EmptyAllowedClientIds_AcceptsRequestWithAnyClientId()
    {
        var urn = "urn:fdc:gov.uk:2022:6fTvD1cMhQNJxrLZSyBgo5";
        var clientSecret = "valid-secret";
        var tokenResponse = new Model.TokenResponse
        {
            AccessToken = "tempToken",
            ExpiresIn = 3600,
            TokenType = "Bearer",
            RefreshToken = "refreshToken",
            RefreshTokenExpiresIn = 36000
        };

        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock.Setup(s => s.ValidateOneLoginToken(clientSecret)).ReturnsAsync((true, urn));
        tokenServiceMock.Setup(s => s.CreateToken(urn)).ReturnsAsync(tokenResponse);

        // Empty allow-list = any client_id (or none) is accepted
        var client = CreateClient([], tokenServiceMock);

        var formData = new Dictionary<string, string>
        {
            { "grant_type", GrantTypes.ClientCredentials },
            { "client_id", "any-arbitrary-client" },
            { "client_secret", clientSecret }
        };

        var response = await client.PostAsync("/token", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
