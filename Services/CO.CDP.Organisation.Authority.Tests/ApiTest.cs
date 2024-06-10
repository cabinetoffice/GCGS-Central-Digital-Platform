using CO.CDP.Organisation.Authority.Model;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CO.CDP.Organisation.Authority.Tests;

public class ApiTest
{
    private readonly string _issuer = "https://issuer.com";
    private readonly string _oneLoginIssuer = "https://onelogin-authority.com";
    private readonly HttpClient _client;
    private readonly Mock<IOpenIdConfiguration> _oneLoginConfiguration = new();

    public ApiTest()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(
                    [
                        new ("Issuer",  _issuer),
                        new ("PublicKey", "-----BEGIN RSA PUBLIC KEY-----\nMIIBCgKCAQEA0ND+OiUI9B/q2EARaXJzb8HaBUduUtkvxj7Qyb+5yw8JZ2/TO3aA0La6km+TLkBla9hwG8+NWTpx2tq+42YQU3L2274GI7dfUS8hqpJ6141PmchaO/10\ndII5e8GtRnaHqY8vWz5I03nPM9gYAT5h7bAlltN0scqCUnmvRmu1A0E5y65OT63o\nQIUAo9PpVqckxM0yHgYKlf6mtTu3dkd8LuhciRnNGaJ5/OGEeyeZrzBzgvE2zmrH\n3H66Oq9dkvr5K/Unhd7Lo4EdGyaI31nRSRshY7F/wVSIgsebya9tGHalh1pgSzL0\nqr9v1+WU5gG/XKvpFuIeByhSWvjsZJykiwIDAQAB\n-----END RSA PUBLIC KEY-----"),
                        new ("PrivateKey", "-----BEGIN RSA PRIVATE KEY-----\nMIIEowIBAAKCAQEA0ND+OiUI9B/q2EARaXJzb8HaBUduUtkvxj7Qyb+5yw8JZ2/T\nO3aA0La6km+TLkBla9hwG8+NWTpx2tq+42YQU3L2274GI7dfUS8hqpJ6141Pmcha\nO/10dII5e8GtRnaHqY8vWz5I03nPM9gYAT5h7bAlltN0scqCUnmvRmu1A0E5y65O\nT63oQIUAo9PpVqckxM0yHgYKlf6mtTu3dkd8LuhciRnNGaJ5/OGEeyeZrzBzgvE2\nzmrH3H66Oq9dkvr5K/Unhd7Lo4EdGyaI31nRSRshY7F/wVSIgsebya9tGHalh1pg\nSzL0qr9v1+WU5gG/XKvpFuIeByhSWvjsZJykiwIDAQABAoIBAAskMBMq+40sGaU5\njeH9bEzCDcic5Owp2YpGKJVmiqzJV1CuPyv7v6gHvroUBH/mibB+YETrmVQWKKQh\n/JSBwN8+ZA7i1s1OehkI8SoxZt3uOT3CkUqEr9xoG3p9WIWSFAGCudSLwFfXEhod\n5V+znRkjWZKnR6Feif73G/zWUg1svKeirhGZ+652c69lnddq4BVRRBqcgd7z9QDQ\n9aNygcQbj+3JTvXs3H642nCAd2j5NuXnF+QNK1gFg4jSuqz3XtGCELm87rXSpn/m\n1BUbcy7cQl/Le5MX2TGGsDzeKsdTQMCWhacHMEncN216373ceS4FDfoqYpyVi9ho\ntJTWFwkCgYEA8ilHOLwtAkinSVosmpS/h5cvXt724B17z/YaFlFojEunav1LDNnA\nHTNAfrC5jXvA/FSltDO0+S3KL4DH2qoaA1lfimNdjm35VWLP9EhtAc8dWN1BbT3a\nRrHH/WTO+IuyfgTy4K6M8wzU3yrV/B4xxRiciLKZ7GCztVwYSrKV3r8CgYEA3L/k\nlIPSbMKMPdc7rft4Q6sLUaEDcApUbQkD2fKFonqe6cVIQkbyTWGEfVnDnhcBJ9JE\nxNb61n3c1I/ByrNDehqaDVnmFNXLp7JXm0cFzoCG7upZx0soR+2ksl8YRLTnn/ma\nooZiAuuuqt28KURseDdonyY5bpXd5DZ9CH9DOTUCgYAlMMO4aeX7sM7ITCoHf4q+\ntzBWQKXnm3+VW7V78fq9eSz4GHh7O8Hueh2Ql3GX0ga8ef+M4lgL4MVpcDzBT1h5\nZTNwHHyU0Dz3qRpI6QqcuYNHT4upzFMGgm01dFL5BvNduULX0NbiyPi0YW7Frpl0\nLKh1sgBDtkJDOyab6jtsHwKBgFMoNoN75RjzcMEbA3BW+scC+BCYByN9wLASZVEE\n+zJp3tNRbhcJgt2DjtVpqzcyk0zc+Ort0TBb2YR5Yxdo0FJ/EulUpEfVAhL6K+Zi\nqt1PrYTy4z8gINx3uUM4b3vyag7piEcROHrBLdtQDEG/dN0UgmTxkVEHQ79kh1Vc\nG4bBAoGBANgDVFyGJslPgRuXbOwEOHBY5tHmvfY7Z1fF4VEBsAPn+UwtS4W8xuCg\nVQazBEmPaeJt6jx73pT7sGTocP9bUJ134CmI/k32Mu5dViTT6lY09DO2OdauColb\nUIM+sEIcV4mz42lu0A9sta3osaHItHZOGpIj9V1OA/2av/bNeYi6\n-----END RSA PRIVATE KEY-----")
                    ]);
            });

            builder.ConfigureServices(s =>
            {
                s.AddSingleton(_ => _oneLoginConfiguration.Object);
            });
        });

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
            ClaimsSupported = ["sub"]
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
            Keys = [new() {
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
    public async Task PostTokenEndpoint_InvalidRequestType_ReturnsUnsupportedMediaTypeResponse()
    {
        var response = await _client.PostAsync("/token", new StringContent("test"));

        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task PostTokenEndpoint_InvalidClientSecret_ReturnsBadRequestResponse()
    {
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_secret", "your-client-secret" }
        };

        _oneLoginConfiguration.Setup(t => t.Get())
            .ReturnsAsync(GetOneLoginAuthorityConfiguration().Object);

        var response = await _client.PostAsync("/token", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostTokenEndpoint_ValidRequest_ReturnsExpectedToken()
    {
        GenerateTempKeys(out var rsaPrivateKey, out var resPublicParams);
        string tokenString = GenerateTempRequestToken(rsaPrivateKey);

        _oneLoginConfiguration.Setup(t => t.Get())
            .ReturnsAsync(GetOneLoginAuthorityConfiguration(resPublicParams).Object);

        var formData = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_secret", tokenString }
        };

        var response = await _client.PostAsync("/token", new FormUrlEncodedContent(formData));
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResponse.Should().NotBeNull();
        tokenResponse.As<TokenResponse>().Should().BeEquivalentTo(new
        {
            ExpiresIn = 3600,
            TokenType = "Bearer"
        }, options => options.ExcludingMissingMembers());
        tokenResponse.As<TokenResponse>().AccessToken.Should().NotBeNull();
    }

    private string GenerateTempRequestToken(RsaSecurityKey rsaPrivateKey)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                    new Claim("sub", "hello")
                }),
            Expires = DateTime.UtcNow.AddMinutes(3),
            Issuer = _oneLoginIssuer,
            SigningCredentials = new SigningCredentials(rsaPrivateKey, SecurityAlgorithms.RsaSha256)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        return tokenString;
    }

    private static void GenerateTempKeys(out RsaSecurityKey rsaPrivateKey, out RSAParameters resPublicParams)
    {
        RsaKeys.GenerateKeys(out var privateKey, out var publicKey);

        var rsaPrivate = RSA.Create();
        rsaPrivate.ImportFromPem(privateKey);
        rsaPrivateKey = new RsaSecurityKey(rsaPrivate);
        var rsaPublic = RSA.Create();
        rsaPublic.ImportFromPem(publicKey);
        resPublicParams = rsaPublic.ExportParameters(false);
    }

    private Mock<OpenIdConnectConfiguration> GetOneLoginAuthorityConfiguration(RSAParameters? rsaParams = null)
    {
        var signingKeys = new List<SecurityKey>();

        if (rsaParams != null)
        {
            signingKeys.Add(new Microsoft.IdentityModel.Tokens.JsonWebKey
            {
                Kty = "RSA",
                Use = "sig",
                Kid = "1",
                Alg = SecurityAlgorithms.RsaSha256,
                N = Base64UrlEncoder.Encode(rsaParams?.Modulus!),
                E = Base64UrlEncoder.Encode(rsaParams?.Exponent!)
            });
        }
        else
        {
            var rsa = RSA.Create();
            signingKeys.Add(new RsaSecurityKey(rsa) { KeyId = "test-key-id" });
        }

        var mockOpenIdConfig = new Mock<OpenIdConnectConfiguration>();
        mockOpenIdConfig.SetupGet(config => config.Issuer).Returns(_oneLoginIssuer);
        mockOpenIdConfig.SetupGet(config => config.SigningKeys).Returns(signingKeys);
        return mockOpenIdConfig;
    }
}
