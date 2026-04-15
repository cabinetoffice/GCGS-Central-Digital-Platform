using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using CO.CDP.Authentication.Models;
using CO.CDP.Authentication.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace CO.CDP.Authentication.Tests;

public class OidcEventsServiceTests
{
    private readonly Mock<ITokenExchangeService> _tokenExchangeService = new();
    private readonly Mock<ISessionManager> _sessionManager = new();

    private OidcEventsService CreateSut()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_tokenExchangeService.Object);
        services.AddSingleton(_sessionManager.Object);
        return new OidcEventsService(
            services.BuildServiceProvider(),
            new ConfigurationBuilder().Build(),
            NullLogger<OidcEventsService>.Instance);
    }

    [Fact]
    public async Task HandleTokenValidated_WhenCdpJwtContainsCdpClaims_AddsCdpClaimsToClaimsPrincipal()
    {
        var cdpClaimsJson = JsonSerializer.Serialize(new
        {
            UserPrincipalId = "urn:fdc:test",
            Organisations = Array.Empty<object>()
        });
        var accessToken = BuildJwt(new Claim("cdp_claims", cdpClaimsJson));
        var authorityTokens = BuildTokenSet(accessToken);

        _tokenExchangeService
            .Setup(s => s.ExchangeOneLoginTokenAsync("onelogin-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorityTokens);
        _sessionManager
            .Setup(s => s.SetTokensAsync(It.IsAny<HttpContext>(), authorityTokens))
            .Returns(Task.CompletedTask);

        var context = BuildTokenValidatedContext("onelogin-token");
        await CreateSut().OnTokenValidated(context);

        var cdpClaim = context.Principal!.FindFirst("cdp_claims");
        cdpClaim.Should().NotBeNull();
        cdpClaim!.Value.Should().Contain("urn:fdc:test");
    }

    [Fact]
    public async Task HandleTokenValidated_WhenCdpJwtHasNoCdpClaims_DoesNotAddCdpClaimsToPrincipal()
    {
        var accessToken = BuildJwt(new Claim("sub", "urn:fdc:test"));
        var authorityTokens = BuildTokenSet(accessToken);

        _tokenExchangeService
            .Setup(s => s.ExchangeOneLoginTokenAsync("onelogin-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorityTokens);
        _sessionManager
            .Setup(s => s.SetTokensAsync(It.IsAny<HttpContext>(), authorityTokens))
            .Returns(Task.CompletedTask);

        var context = BuildTokenValidatedContext("onelogin-token");
        await CreateSut().OnTokenValidated(context);

        context.Principal!.FindFirst("cdp_claims").Should().BeNull();
    }

    [Fact]
    public async Task HandleTokenValidated_WhenCdpJwtHasCdpClaims_StillStoresTokensInSession()
    {
        var accessToken = BuildJwt(new Claim("cdp_claims", "{}"));
        var authorityTokens = BuildTokenSet(accessToken);

        _tokenExchangeService
            .Setup(s => s.ExchangeOneLoginTokenAsync("onelogin-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorityTokens);
        _sessionManager
            .Setup(s => s.SetTokensAsync(It.IsAny<HttpContext>(), authorityTokens))
            .Returns(Task.CompletedTask);

        var context = BuildTokenValidatedContext("onelogin-token");
        await CreateSut().OnTokenValidated(context);

        _sessionManager.Verify(s => s.SetTokensAsync(It.IsAny<HttpContext>(), authorityTokens), Times.Once);
    }

    [Fact]
    public async Task HandleTokenValidated_WhenNoOneLoginToken_FailsContext()
    {
        var context = BuildTokenValidatedContext(oneLoginToken: null);
        await CreateSut().OnTokenValidated(context);

        context.Result.Should().NotBeNull();
        context.Result.Succeeded.Should().BeFalse();
        _tokenExchangeService.Verify(
            s => s.ExchangeOneLoginTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleTokenValidated_WhenTokenExchangeFails_FailsContextAndDoesNotAddCdpClaims()
    {
        _tokenExchangeService
            .Setup(s => s.ExchangeOneLoginTokenAsync("onelogin-token", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("exchange failed"));

        var context = BuildTokenValidatedContext("onelogin-token");
        await CreateSut().OnTokenValidated(context);

        context.Result.Should().NotBeNull();
        context.Result.Succeeded.Should().BeFalse();
        context.Principal!.FindFirst("cdp_claims").Should().BeNull();
    }

    private static string BuildJwt(params Claim[] claims)
    {
        using var rsa = RSA.Create(2048);
        var key = new RsaSecurityKey(rsa);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "https://authority.test",
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
        };
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    private static AuthorityTokenSet BuildTokenSet(string accessToken) =>
        AuthorityTokenSet.Create(accessToken, "refresh-token", 3600, 86400, DateTimeOffset.UtcNow);

    private static TokenValidatedContext BuildTokenValidatedContext(string? oneLoginToken)
    {
        var httpContext = new DefaultHttpContext();
        var scheme = new AuthenticationScheme(
            "oidc", null, typeof(OpenIdConnectHandler));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sub", "urn:fdc:test")
        }, "test"));

        return new TokenValidatedContext(httpContext, scheme, new OpenIdConnectOptions(), principal, new AuthenticationProperties())
        {
            TokenEndpointResponse = oneLoginToken != null
                ? new OpenIdConnectMessage { AccessToken = oneLoginToken }
                : null
        };
    }
}
