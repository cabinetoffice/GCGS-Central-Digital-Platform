using CO.CDP.Organisation.Authority.Model;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using IdentityModel;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CO.CDP.Organisation.Authority.Tests;

public class TokenServiceTest
{
    private readonly string _issuer = "https://issuer.com";
    private readonly string _oneLoginIssuer = "https://onelogin-authority.com";
    private readonly string _userUrn = "urn:fdc:gov.uk:2022:6fTvD1cMhQNJxrLZSyBgo5";

    private readonly Mock<ILogger<TokenService>> _loggerMock = new();
    private readonly Mock<IConfigurationService> _configServiceMock = new();
    private readonly Mock<IPersonRepository> _personRepositoryMock = new();
    private readonly Mock<IAuthorityRepository> _authorityRepositoryMock = new();
    private readonly TokenService _tokenService;

    public TokenServiceTest()
    {
        _tokenService = new TokenService(
            _loggerMock.Object,
            _configServiceMock.Object,
             _personRepositoryMock.Object,
            _authorityRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateToken_ShouldReturnValidToken()
    {
        GenerateTempKeys(out var rsaPrivateKey, out var resPublicParams);
        _configServiceMock.Setup(c => c.GetAuthorityConfiguration())
            .Returns(new AuthorityConfiguration { Issuer = _issuer, RsaPrivateKey = rsaPrivateKey, RsaPublicParams = resPublicParams });

        var result = await _tokenService.CreateToken(_userUrn);

        result.Should().NotBeNull();
        result.TokenType.Should().Be(OidcConstants.TokenResponse.BearerTokenType);
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.ExpiresIn.Should().Be(3600);
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshTokenExpiresIn.Should().Be(86400d);

        var claims = GetClaims(result.AccessToken);
        claims.FirstOrDefault(c => c.Type == "sub")?.Value.Should().Be(_userUrn);
    }

    [Fact]
    public async Task ValidateOneLoginToken_ShouldReturnTrueAndUrn_ForValidToken()
    {
        GenerateTempKeys(out var rsaPrivateKey, out var rsaPublicParams);
        string token = GenerateOneLoginToken(rsaPrivateKey, _userUrn);

        _configServiceMock.Setup(c => c.GetOneLoginConfiguration(false))
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

        _configServiceMock.Setup(c => c.GetOneLoginConfiguration(false))
            .ReturnsAsync(GetOneLoginAuthorityConfiguration().Object);

        _configServiceMock.Setup(c => c.GetOneLoginConfiguration(true))
            .ReturnsAsync(GetOneLoginAuthorityConfiguration().Object);

        (bool valid, string? urn) = await _tokenService.ValidateOneLoginToken(token);

        valid.Should().BeFalse();
        urn.Should().BeNull();
    }

    [Fact]
    public async Task ValidateOneLoginToken_ShouldReturnTrueAndUrn_WhenMatchingSigningKeysOnRetry()
    {
        GenerateTempKeys(out var rsaPrivateKey, out var rsaPublicParams);
        string token = GenerateOneLoginToken(rsaPrivateKey, _userUrn);

        _configServiceMock.Setup(c => c.GetOneLoginConfiguration(false))
            .ReturnsAsync(GetOneLoginAuthorityConfiguration().Object);

        _configServiceMock.Setup(c => c.GetOneLoginConfiguration(true))
            .ReturnsAsync(GetOneLoginAuthorityConfiguration(rsaPublicParams).Object);

        (bool valid, string? urn) = await _tokenService.ValidateOneLoginToken(token);

        valid.Should().BeTrue();
        urn.Should().Be(_userUrn);

        _configServiceMock.Verify(c => c.GetOneLoginConfiguration(false), Times.Once);
        _configServiceMock.Verify(c => c.GetOneLoginConfiguration(true), Times.Once);
    }

    [Fact]
    public async Task ValidateOneLoginToken_ShouldReturnFalseAndNullUrn_WhenTokenIsNull()
    {
        (bool valid, string? urn) = await _tokenService.ValidateOneLoginToken(null);

        valid.Should().BeFalse();
        urn.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAndRevokeRefreshToken_ShouldReturnValidWhenTokenIsCorrect()
    {
        string token = "password:dmFsaWQtc2FsdA==";
        string hashToValidate = "RfLTQtUMjxyhUSKTJOzTW6PIPv+zRvvWTM1ylJlnNS8=";

        _authorityRepositoryMock.Setup(x => x.Find(hashToValidate))
            .ReturnsAsync(new RefreshToken { TokenHash = hashToValidate, ExpiryDate = DateTimeOffset.Now.AddMinutes(1) });

        var (valid, urn) = await _tokenService.ValidateAndRevokeRefreshToken(token);

        valid.Should().BeTrue();
        urn.Should().Be("valid-salt");
    }

    [Fact]
    public async Task ValidateAndRevokeRefreshToken_ShouldReturnInvalidWhenTokenIsIncorrect()
    {
        string token = "invalid-token";

        var result = await _tokenService.ValidateAndRevokeRefreshToken(token);

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

    private static IEnumerable<Claim> GetClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
        return jsonToken.Claims;
    }
}
