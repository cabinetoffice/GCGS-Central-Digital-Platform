using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using CO.CDP.Authentication;
using CO.CDP.Authentication.Models;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ApiClient = CO.CDP.UserManagement.WebApiClient;

namespace CO.CDP.UserManagement.App.Tests.Authorization;

/// <summary>
/// Shared test infrastructure for organisation authorization handler tests.
/// </summary>
public abstract class AuthorizationHandlerTestBase
{
    protected const string TestSlug = "test-org";
    protected static readonly Guid TestOrgId = Guid.NewGuid();

    protected readonly Mock<ApiClient.UserManagementClient> ApiClient =
        new("http://localhost", new HttpClient());

    protected readonly Mock<ISessionManager> SessionManager = new();

    protected static OrganisationResponse BuildOrgResponse(Guid? orgId = null) =>
        new()
        {
            Id = 1,
            CdpOrganisationGuid = orgId ?? TestOrgId,
            Name = "Test Organisation",
            Slug = TestSlug,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

    /// <summary>Build cdp_claims JSON for one org membership.</summary>
    protected static string BuildCdpClaimsJson(Guid orgId, string role) =>
        JsonSerializer.Serialize(new UserClaims
        {
            UserPrincipalId = "urn:fdc:test",
            Organisations =
            [
                new OrganisationMembershipClaim
                {
                    OrganisationId = orgId,
                    OrganisationName = "Test Organisation",
                    OrganisationRole = role
                }
            ]
        });

    /// <summary>Build a real JWT access token containing a cdp_claims claim.</summary>
    protected static string BuildCdpJwt(string cdpClaimsJson)
    {
        using var rsa = RSA.Create(2048);
        var key = new RsaSecurityKey(rsa);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("cdp_claims", cdpClaimsJson)]),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "https://authority.test",
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
        };
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    /// <summary>Create an HttpContext with route data for the given slug.</summary>
    protected static HttpContext BuildHttpContext(string slug = TestSlug)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.RouteValues = new RouteValueDictionary { ["organisationSlug"] = slug };
        return ctx;
    }

    /// <summary>Create a ClaimsPrincipal with cdp_claims populated (primary path).</summary>
    protected static ClaimsPrincipal BuildPrincipalWithCdpClaims(string cdpClaimsJson) =>
        new(new ClaimsIdentity(
            [new Claim("cdp_claims", cdpClaimsJson, Microsoft.IdentityModel.JsonWebTokens.JsonClaimValueTypes.Json)],
            "test"));

    /// <summary>Create a ClaimsPrincipal WITHOUT cdp_claims (tests fallback path).</summary>
    protected static ClaimsPrincipal BuildPrincipalNoClaims() =>
        new(new ClaimsIdentity([new Claim("sub", "urn:fdc:test")], "test"));

    /// <summary>Configure SessionManager to return a real CDP JWT containing cdp_claims.</summary>
    protected void SetupSessionWithCdpJwt(string cdpClaimsJson)
    {
        var tokenSet = AuthorityTokenSet.Create(
            BuildCdpJwt(cdpClaimsJson), "refresh", 3600, 86400, DateTimeOffset.UtcNow);
        SessionManager
            .Setup(s => s.GetTokensAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(tokenSet);
    }

    protected void SetupSessionReturnsNull() =>
        SessionManager
            .Setup(s => s.GetTokensAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((AuthorityTokenSet?)null);
}