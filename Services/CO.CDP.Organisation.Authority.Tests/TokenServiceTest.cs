using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CO.CDP.Organisation.Authority.Model;
using CO.CDP.Organisation.Authority.Tests.AutoMapper;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using IdentityModel;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;
using TenantLookup = CO.CDP.OrganisationInformation.Persistence.TenantLookup;

namespace CO.CDP.Organisation.Authority.Tests;

public class TokenServiceTest : IClassFixture<AutoMapperFixture>
{
    private readonly string _issuer = "https://issuer.com";
    private readonly string _oneLoginIssuer = "https://onelogin-authority.com";
    private readonly string _userUrn = "urn:fdc:gov.uk:2022:6fTvD1cMhQNJxrLZSyBgo5";

    private readonly Mock<ILogger<TokenService>> _loggerMock;
    private readonly Mock<IConfigurationService> _configServiceMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IAuthorityRepository> _authorityRepositoryMock;
    private readonly TokenService _tokenService;

    public TokenServiceTest(AutoMapperFixture mapperFixture)
    {
        _loggerMock = new();
        _configServiceMock = new();
        _tenantRepositoryMock = new();
        _authorityRepositoryMock = new();

        _tokenService = new TokenService(
            _loggerMock.Object,
            _configServiceMock.Object,
             mapperFixture.Mapper,
            _tenantRepositoryMock.Object,
            _authorityRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateToken_WhenNoTenantLookup_ShouldReturnValidTokenResponseWithoutTenantLookupInfo()
    {
        GenerateTempKeys(out var rsaPrivateKey, out var resPublicParams);
        _configServiceMock.Setup(c => c.GetAuthorityConfiguration())
            .Returns(new AuthorityConfiguration { Issuer = _issuer, RsaPrivateKey = rsaPrivateKey, RsaPublicParams = resPublicParams });

        _tenantRepositoryMock.Setup(t => t.LookupTenant(_userUrn)).ReturnsAsync((TenantLookup?)null);

        var result = await _tokenService.CreateToken(_userUrn);

        result.Should().NotBeNull();
        result.TokenType.Should().Be(OidcConstants.TokenResponse.BearerTokenType);
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.ExpiresIn.Should().Be(3600);
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshTokenExpiresIn.Should().Be(86400d);

        var claims = GetClaims(result.AccessToken);
        claims.FirstOrDefault(c => c.Type == "sub")?.Value.Should().Be(_userUrn);
        claims.FirstOrDefault(c => c.Type == "ten")?.Value.Should().BeNull();
    }

    [Fact]
    public async Task CreateToken_WhenTenantLookup_ShouldReturnValidTokenResponseWithTenantLookupInfo()
    {
        GenerateTempKeys(out var rsaPrivateKey, out var resPublicParams);
        _configServiceMock.Setup(c => c.GetAuthorityConfiguration())
            .Returns(new AuthorityConfiguration { Issuer = _issuer, RsaPrivateKey = rsaPrivateKey, RsaPublicParams = resPublicParams });

        _tenantRepositoryMock.Setup(t => t.LookupTenant(_userUrn))
            .ReturnsAsync(GetTenantLookup());

        var result = await _tokenService.CreateToken(_userUrn);

        result.Should().NotBeNull();
        result.TokenType.Should().Be(OidcConstants.TokenResponse.BearerTokenType);
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.ExpiresIn.Should().Be(3600);
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshTokenExpiresIn.Should().Be(86400d);

        var claims = GetClaims(result.AccessToken);
        claims.FirstOrDefault(c => c.Type == "sub")?.Value.Should().Be(_userUrn);
        claims.FirstOrDefault(c => c.Type == "ten")?.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateToken_WhenTenantLookupWithMoreThanTenOrganisation_ShouldReturnValidTokenResponseWithTenOrganisation()
    {
        GenerateTempKeys(out var rsaPrivateKey, out var resPublicParams);
        _configServiceMock.Setup(c => c.GetAuthorityConfiguration())
            .Returns(new AuthorityConfiguration { Issuer = _issuer, RsaPrivateKey = rsaPrivateKey, RsaPublicParams = resPublicParams });

        _tenantRepositoryMock.Setup(t => t.LookupTenant(_userUrn))
            .ReturnsAsync(GetTenantLookup(6, 4));

        var result = await _tokenService.CreateToken(_userUrn);

        result.Should().NotBeNull();
        result.TokenType.Should().Be(OidcConstants.TokenResponse.BearerTokenType);
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.ExpiresIn.Should().Be(3600);
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshTokenExpiresIn.Should().Be(86400d);

        var claims = GetClaims(result.AccessToken);
        claims.FirstOrDefault(c => c.Type == "sub")?.Value.Should().Be(_userUrn);

        var tenant = claims.FirstOrDefault(c => c.Type == "ten")?.Value;
        tenant.Should().NotBeNull();
        var lookup = JsonSerializer.Deserialize<TenantLookup>(Encoding.UTF8.GetString(Decompress(Convert.FromBase64String(tenant!))));
        lookup!.Tenants.SelectMany(t => t.Organisations).Count().Should().Be(10);
    }

    [Fact]
    public async Task ValidateOneLoginToken_ShouldReturnTrueAndUrn_ForValidToken()
    {
        GenerateTempKeys(out var rsaPrivateKey, out var rsaPublicParams);
        string token = GenerateOneLoginToken(rsaPrivateKey, _userUrn);

        _configServiceMock.Setup(c => c.GetOneLoginConfiguration())
            .ReturnsAsync(GetOneLoginAuthorityConfiguration(rsaPublicParams).Object);

        (bool valid, string? urn) = await _tokenService.ValidateOneLoginToken(token);

        valid.Should().BeTrue();
        urn.Should().Be(_userUrn);
    }

    [Fact]
    public async Task ValidateOneLoginToken_ShouldReturnFalseAndNullUrn_WhenNoMatchingSigningKeys()
    {
        GenerateTempKeys(out var rsaPrivateKey, out _);
        string token = GenerateOneLoginToken(rsaPrivateKey, _userUrn);

        _configServiceMock.Setup(c => c.GetOneLoginConfiguration())
            .ReturnsAsync(GetOneLoginAuthorityConfiguration().Object);

        (bool valid, string? urn) = await _tokenService.ValidateOneLoginToken(token);

        valid.Should().BeFalse();
        urn.Should().BeNull();
    }

    [Fact]
    public async Task ValidateOneLoginToken_ShouldReturnFalseAndNullUrn_WhenTokenIsNull()
    {
        (bool valid, string? urn) = await _tokenService.ValidateOneLoginToken(null);

        valid.Should().BeFalse();
        urn.Should().BeNull();
    }

    [Fact]
    public async Task ValidateRefreshToken_ShouldReturnValidWhenTokenIsCorrect()
    {
        string token = "password:dmFsaWQtc2FsdA==";
        string hashToValidate = "RfLTQtUMjxyhUSKTJOzTW6PIPv+zRvvWTM1ylJlnNS8=";

        _authorityRepositoryMock.Setup(x => x.Find(hashToValidate))
            .ReturnsAsync(new RefreshToken { TokenHash = hashToValidate, ExpiryDate = DateTime.Now.AddMinutes(1) });

        var (valid, urn) = await _tokenService.ValidateRefreshToken(token);

        valid.Should().BeTrue();
        urn.Should().Be("valid-salt");
    }

    [Fact]
    public async Task ValidateRefreshToken_ShouldReturnInvalidWhenTokenIsIncorrect()
    {
        string token = "invalid-token";

        var result = await _tokenService.ValidateRefreshToken(token);

        result.valid.Should().BeFalse();
        result.urn.Should().BeNull();
    }

    private string GenerateOneLoginToken(RsaSecurityKey rsaPrivateKey, string? urn = null)
    {
        List<Claim> claims = [];
        if (urn != null) claims.Add(new Claim("sub", urn));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(3),
            Issuer = _oneLoginIssuer,
            SigningCredentials = new SigningCredentials(rsaPrivateKey, SecurityAlgorithms.RsaSha256)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }

    private static void GenerateTempKeys(out RsaSecurityKey rsaPrivateKey, out RSAParameters rsaPublicParams)
    {
        RsaKeys.GenerateKeys(out var privateKey, out _);

        var rsaPrivate = RSA.Create();
        rsaPrivate.ImportFromPem(privateKey);
        rsaPrivateKey = new RsaSecurityKey(rsaPrivate);

        var rsaPublic = RSA.Create();
        rsaPublic.ImportFromPem(rsaPrivate.ExportRSAPublicKeyPem());
        rsaPublicParams = rsaPublic.ExportParameters(false);
    }

    private Mock<OpenIdConnectConfiguration> GetOneLoginAuthorityConfiguration(RSAParameters? rsaParams = null)
    {
        var signingKeys = new List<SecurityKey>();

        if (rsaParams != null)
        {
            signingKeys.Add(new JsonWebKey
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

    private TenantLookup GetTenantLookup(int numberOfTenant = 1, int numberOfOrgsPerTenant = 1)
    {
        var tenantLookup = new TenantLookup
        {
            User = new TenantLookup.PersonUser
            {
                Email = "test@test.com",
                Name = "Test person",
                Urn = _userUrn
            },
            Tenants = Enumerable.Range(0, numberOfTenant).Select(x =>
                new TenantLookup.Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = $"Test Tenant{x}",
                    Organisations = Enumerable.Range(0, numberOfOrgsPerTenant).Select(y =>
                    new TenantLookup.Organisation
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Test Org{x}{y}",
                        Roles = [PartyRole.Buyer],
                        Scopes = ["Admin"]
                    }).ToList()
                }).ToList()
        };

        return tenantLookup;
    }

    private static byte[] Decompress(byte[] compressedData)
    {
        using var uncompressedStream = new MemoryStream();

        using (var compressedStream = new MemoryStream(compressedData))
        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress, false))
        {
            gzipStream.CopyTo(uncompressedStream);
        }

        return uncompressedStream.ToArray();
    }

    private static IEnumerable<Claim> GetClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
        return jsonToken.Claims;
    }
}