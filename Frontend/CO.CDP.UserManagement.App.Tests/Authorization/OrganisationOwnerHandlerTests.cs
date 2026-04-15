using CO.CDP.UserManagement.App.Authorization.Handlers;
using CO.CDP.UserManagement.App.Authorization.Requirements;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ApiClient = CO.CDP.UserManagement.WebApiClient;

namespace CO.CDP.UserManagement.App.Tests.Authorization;

public class OrganisationOwnerHandlerTests : AuthorizationHandlerTestBase
{
    private OrganisationOwnerHandler CreateSut() =>
        new(ApiClient.Object, SessionManager.Object, NullLogger<OrganisationOwnerHandler>.Instance);

    [Fact]
    public async Task HandleRequirementAsync_WhenCdpClaimsInPrincipal_AndUserIsOwner_Succeeds()
    {
        var claimsJson = BuildCdpClaimsJson(TestOrgId, "Owner");
        ApiClient.Setup(c => c.BySlugAsync(TestSlug))
            .ReturnsAsync(BuildOrgResponse());

        var context = BuildAuthContext(
            BuildPrincipalWithCdpClaims(claimsJson),
            BuildHttpContext());

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenCdpClaimsOnlyInSessionJwt_AndUserIsOwner_Succeeds()
    {
        var claimsJson = BuildCdpClaimsJson(TestOrgId, "Owner");
        SetupSessionWithCdpJwt(claimsJson);
        ApiClient.Setup(c => c.BySlugAsync(TestSlug))
            .ReturnsAsync(BuildOrgResponse());

        var context = BuildAuthContext(
            BuildPrincipalNoClaims(),
            BuildHttpContext());

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenCdpClaimsAbsentEverywhere_DoesNotSucceed()
    {
        SetupSessionReturnsNull();

        var context = BuildAuthContext(BuildPrincipalNoClaims(), BuildHttpContext());

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsAdmin_DoesNotSucceed()
    {
        var claimsJson = BuildCdpClaimsJson(TestOrgId, "Admin");
        ApiClient.Setup(c => c.BySlugAsync(TestSlug))
            .ReturnsAsync(BuildOrgResponse());

        var context = BuildAuthContext(
            BuildPrincipalWithCdpClaims(claimsJson),
            BuildHttpContext());

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenOrganisationSlugMissing_DoesNotSucceed()
    {
        var claimsJson = BuildCdpClaimsJson(TestOrgId, "Owner");
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext(); // no route data

        var context = BuildAuthContext(BuildPrincipalWithCdpClaims(claimsJson), httpContext);

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenApiThrows_DoesNotSucceed()
    {
        var claimsJson = BuildCdpClaimsJson(TestOrgId, "Owner");
        ApiClient.Setup(c => c.BySlugAsync(TestSlug))
            .ThrowsAsync(new ApiClient.ApiException("not found", 404, "", new Dictionary<string, IEnumerable<string>>(), null));

        var context = BuildAuthContext(
            BuildPrincipalWithCdpClaims(claimsJson),
            BuildHttpContext());

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private static AuthorizationHandlerContext BuildAuthContext(
        System.Security.Claims.ClaimsPrincipal principal,
        Microsoft.AspNetCore.Http.HttpContext httpContext) =>
        new([new OrganisationOwnerRequirement()], principal, httpContext);
}
