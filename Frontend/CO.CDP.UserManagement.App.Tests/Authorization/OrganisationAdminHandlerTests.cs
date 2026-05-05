using CO.CDP.UserManagement.App.Authorization.Handlers;
using CO.CDP.UserManagement.App.Authorization.Requirements;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging.Abstractions;

namespace CO.CDP.UserManagement.App.Tests.Authorization;

public class OrganisationAdminHandlerTests : AuthorizationHandlerTestBase
{
    private OrganisationAdminHandler CreateSut() =>
        new(SessionManager.Object, NullLogger<OrganisationAdminHandler>.Instance);

    [Fact]
    public async Task HandleRequirementAsync_WhenCdpClaimsInPrincipal_AndUserIsAdmin_Succeeds()
    {
        var claimsJson = BuildCdpClaimsJson(TestOrgId, "Admin");
        var context = BuildAuthContext(
            BuildPrincipalWithCdpClaims(claimsJson),
            BuildHttpContext());

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenCdpClaimsOnlyInSessionJwt_AndUserIsAdmin_Succeeds()
    {
        var claimsJson = BuildCdpClaimsJson(TestOrgId, "Admin");
        SetupSessionWithCdpJwt(claimsJson);

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
    public async Task HandleRequirementAsync_WhenUserIsOwner_DoesNotSucceed()
    {
        var claimsJson = BuildCdpClaimsJson(TestOrgId, "Owner");
        var context = BuildAuthContext(
            BuildPrincipalWithCdpClaims(claimsJson),
            BuildHttpContext());

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenOrganisationIdMissing_DoesNotSucceed()
    {
        var claimsJson = BuildCdpClaimsJson(TestOrgId, "Admin");
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        var context = BuildAuthContext(BuildPrincipalWithCdpClaims(claimsJson), httpContext);

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private static AuthorizationHandlerContext BuildAuthContext(
        System.Security.Claims.ClaimsPrincipal principal,
        Microsoft.AspNetCore.Http.HttpContext httpContext) =>
        new([new OrganisationAdminRequirement()], principal, httpContext);
}
