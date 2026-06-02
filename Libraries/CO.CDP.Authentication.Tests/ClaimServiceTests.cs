using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication.Tests;

public class ClaimServiceTests
{
    private readonly Mock<IOrganisationRepository> mockOrgRepo = new();

    [Fact]
    public void GetUserUrn_ShouldReturnUrn_WhenUserHasSubClaim()
    {
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var httpContextAccessor = GivenHttpContextWith([new(ClaimType.Subject, userUrn)]);

        var claimService = new ClaimService(httpContextAccessor.Object, mockOrgRepo.Object);

        var result = claimService.GetUserUrn();
        result.Should().Be(userUrn);
    }

    [Fact]
    public void GetUserUrn_ShouldReturnNull_WhenUserHasNoSubClaim()
    {
        var httpContextAccessor = GivenHttpContextWith([]);

        var claimService = new ClaimService(httpContextAccessor.Object, mockOrgRepo.Object);
        var result = claimService.GetUserUrn();

        result.Should().BeNull();
    }

    [Fact]
    public async Task HaveAccessToOrganisation_ShouldReturnFalse_WhenUserHasNoSubClaim()
    {
        var httpContextAccessor = GivenHttpContextWith([]);

        var claimService = new ClaimService(httpContextAccessor.Object, mockOrgRepo.Object);
        var result = await claimService.HaveAccessToOrganisation(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HaveAccessToOrganisation_ShouldReturnFalse_WhenUserHasNoTenant()
    {
        var organisationId = Guid.NewGuid();
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var httpContextAccessor = GivenHttpContextWith([new(ClaimType.Subject, userUrn)]);

        mockOrgRepo.Setup(m => m.FindOrganisationPerson(organisationId, userUrn))
            .ReturnsAsync((OrganisationPerson?)default);

        var claimService = new ClaimService(httpContextAccessor.Object, mockOrgRepo.Object);
        var result = await claimService.HaveAccessToOrganisation(organisationId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HaveAccessToOrganisation_ShouldReturnTrue_WhenDoesHaveAccessToOrganisation()
    {
        var organisationId = new Guid("96dc0f35-c059-4d89-91fa-a6ba5e4861a2");
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var httpContextAccessor = GivenHttpContextWith([new(ClaimType.Subject, userUrn)]);

        mockOrgRepo.Setup(m => m.FindOrganisationPerson(organisationId, userUrn))
            .ReturnsAsync(new OrganisationPerson
            {
                Organisation = Mock.Of<OrganisationInformation.Persistence.Organisation>(),
                Person = Mock.Of<OrganisationInformation.Persistence.Person>(),
                Scopes = ["Admin"]
            });

        var claimService = new ClaimService(httpContextAccessor.Object, mockOrgRepo.Object);
        var result = await claimService.HaveAccessToOrganisation(organisationId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HaveAccessToOrganisation_ShouldReturnTrue_WhenUserIsSupportAdmin()
    {
        var organisationId = new Guid("96dc0f35-c059-4d89-91fa-a6ba5e4861a2");
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var httpContextAccessor = GivenHttpContextWith([new(ClaimType.Subject, userUrn)]);

        mockOrgRepo.Setup(m => m.FindOrganisationPerson(organisationId, userUrn))
            .ReturnsAsync(new OrganisationPerson
            {
                Organisation = Mock.Of<OrganisationInformation.Persistence.Organisation>(),
                Person = Mock.Of<OrganisationInformation.Persistence.Person>(),
                Scopes = []
            });

        var claimService = new ClaimService(httpContextAccessor.Object, mockOrgRepo.Object);
        var result = await claimService.HaveAccessToOrganisation(organisationId, null, [PersonScope.SupportAdmin]);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HaveAccessToOrganisation_ShouldReturnTrue_WhenUserIsSuperAdmin()
    {
        var organisationId = new Guid("96dc0f35-c059-4d89-91fa-a6ba5e4861a2");
        var userUrn = "urn:fdc:gov.uk:2022:rynbwxUssDAcmU38U5gxd7dBfu9N7KFP9_nqDuZ66Hg";
        var httpContextAccessor = GivenHttpContextWith([new(ClaimType.Subject, userUrn)]);

        mockOrgRepo.Setup(m => m.FindOrganisationPerson(organisationId, userUrn))
            .ReturnsAsync(new OrganisationPerson
            {
                Organisation = Mock.Of<OrganisationInformation.Persistence.Organisation>(),
                Person = Mock.Of<OrganisationInformation.Persistence.Person>(),
                Scopes = []
            });

        var claimService = new ClaimService(httpContextAccessor.Object, mockOrgRepo.Object);
        var result = await claimService.HaveAccessToOrganisation(organisationId, null, [PersonScope.SuperAdmin]);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetChannel_ShouldReturnUrn_WhenUserHasChannelClaim()
    {
        var channel = "onelogin";
        var httpContextAccessor = GivenHttpContextWith([new(ClaimType.Channel, channel)]);

        var claimService = new ClaimService(httpContextAccessor.Object, mockOrgRepo.Object);

        var result = claimService.GetChannel();
        result.Should().Be(channel);
    }

    [Fact]
    public void GetChannel_ShouldReturnNull_WhenUserHasNoChannelClaim()
    {
        var httpContextAccessor = GivenHttpContextWith([]);

        var claimService = new ClaimService(httpContextAccessor.Object, mockOrgRepo.Object);
        var result = claimService.GetChannel();

        result.Should().BeNull();
    }

    // ── GetApplicationClaims (Phase 3B) ───────────────────────────────────

    [Fact]
    public void GetApplicationClaims_ShouldReturn_DeserializedClaims_WhenClaimPresent()
    {
        const string claimsJson =
            """{"userPrincipalId":"urn:test","organisations":[{"organisationId":"00000000-0000-0000-0000-000000000001","organisationName":"Org1","organisationRole":"Admin","applications":[{"applicationId":"00000000-0000-0000-0000-000000000002","applicationName":"FTS","clientId":"fts-app","roles":["Buyer"],"permissions":["submit:notice"]}]}]}""";

        var ctx = GivenHttpContextWith([new(ClaimType.CdpClaims, claimsJson)]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        var result = svc.GetApplicationClaims();

        result.Should().NotBeNull();
        result!.UserPrincipalId.Should().Be("urn:test");
        result.Organisations.Should().HaveCount(1);
        result.Organisations.First().Applications.Should().HaveCount(1);
        result.Organisations.First().Applications.First().ClientId.Should().Be("fts-app");
    }

    [Fact]
    public void GetApplicationClaims_ShouldReturn_Null_WhenClaimAbsent()
    {
        var ctx = GivenHttpContextWith([]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        svc.GetApplicationClaims().Should().BeNull();
    }

    [Fact]
    public void GetApplicationClaims_ShouldReturn_Null_WhenJsonInvalid()
    {
        var ctx = GivenHttpContextWith([new(ClaimType.CdpClaims, "not-valid-json{{{")]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        svc.GetApplicationClaims().Should().BeNull();
    }

    // ── HasApplicationRole (Phase 3B) ─────────────────────────────────────

    [Fact]
    public void HasApplicationRole_ShouldReturnTrue_WhenRolePresentForOrgAndClient()
    {
        var orgId = new Guid("00000000-0000-0000-0000-000000000001");
        const string json =
            """{"userPrincipalId":"urn:test","organisations":[{"organisationId":"00000000-0000-0000-0000-000000000001","organisationName":"O","organisationRole":"Admin","applications":[{"applicationId":"00000000-0000-0000-0000-000000000002","applicationName":"FTS","clientId":"fts-app","roles":["Buyer"],"permissions":[]}]}]}""";

        var ctx = GivenHttpContextWith([new(ClaimType.CdpClaims, json)]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        svc.HasApplicationRole(orgId, "fts-app", "Buyer").Should().BeTrue();
    }

    [Fact]
    public void HasApplicationRole_ShouldReturnFalse_WhenRoleNotPresent()
    {
        var orgId = new Guid("00000000-0000-0000-0000-000000000001");
        const string json =
            """{"userPrincipalId":"urn:test","organisations":[{"organisationId":"00000000-0000-0000-0000-000000000001","organisationName":"O","organisationRole":"Admin","applications":[{"applicationId":"00000000-0000-0000-0000-000000000002","applicationName":"FTS","clientId":"fts-app","roles":["Reviewer"],"permissions":[]}]}]}""";

        var ctx = GivenHttpContextWith([new(ClaimType.CdpClaims, json)]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        svc.HasApplicationRole(orgId, "fts-app", "Buyer").Should().BeFalse();
    }

    [Fact]
    public void HasApplicationRole_ShouldReturnFalse_WhenNoCdpClaimsClaim()
    {
        var ctx = GivenHttpContextWith([]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        svc.HasApplicationRole(Guid.NewGuid(), "any-app", "AnyRole").Should().BeFalse();
    }

    [Fact]
    public void HasApplicationRole_ShouldReturnFalse_WhenWrongOrganisation()
    {
        const string json =
            """{"userPrincipalId":"urn:test","organisations":[{"organisationId":"00000000-0000-0000-0000-000000000001","organisationName":"O","organisationRole":"Admin","applications":[{"applicationId":"00000000-0000-0000-0000-000000000002","applicationName":"FTS","clientId":"fts-app","roles":["Buyer"],"permissions":[]}]}]}""";

        var ctx = GivenHttpContextWith([new(ClaimType.CdpClaims, json)]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        // Different orgId — should not match
        svc.HasApplicationRole(Guid.NewGuid(), "fts-app", "Buyer").Should().BeFalse();
    }

    [Fact]
    public void HasApplicationRole_ShouldReturnFalse_WhenWrongClientId()
    {
        var orgId = new Guid("00000000-0000-0000-0000-000000000001");
        const string json =
            """{"userPrincipalId":"urn:test","organisations":[{"organisationId":"00000000-0000-0000-0000-000000000001","organisationName":"O","organisationRole":"Admin","applications":[{"applicationId":"00000000-0000-0000-0000-000000000002","applicationName":"FTS","clientId":"fts-app","roles":["Buyer"],"permissions":[]}]}]}""";

        var ctx = GivenHttpContextWith([new(ClaimType.CdpClaims, json)]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        svc.HasApplicationRole(orgId, "other-app", "Buyer").Should().BeFalse();
    }

    // ── HasApplicationPermission (Phase 3B) ───────────────────────────────

    [Fact]
    public void HasApplicationPermission_ShouldReturnTrue_WhenPermissionPresent()
    {
        var orgId = new Guid("00000000-0000-0000-0000-000000000001");
        const string json =
            """{"userPrincipalId":"urn:test","organisations":[{"organisationId":"00000000-0000-0000-0000-000000000001","organisationName":"O","organisationRole":"Admin","applications":[{"applicationId":"00000000-0000-0000-0000-000000000002","applicationName":"FTS","clientId":"fts-app","roles":[],"permissions":["submit:notice"]}]}]}""";

        var ctx = GivenHttpContextWith([new(ClaimType.CdpClaims, json)]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        svc.HasApplicationPermission(orgId, "fts-app", "submit:notice").Should().BeTrue();
    }

    [Fact]
    public void HasApplicationPermission_ShouldReturnFalse_WhenPermissionMissing()
    {
        var orgId = new Guid("00000000-0000-0000-0000-000000000001");
        const string json =
            """{"userPrincipalId":"urn:test","organisations":[{"organisationId":"00000000-0000-0000-0000-000000000001","organisationName":"O","organisationRole":"Admin","applications":[{"applicationId":"00000000-0000-0000-0000-000000000002","applicationName":"FTS","clientId":"fts-app","roles":[],"permissions":["read:data"]}]}]}""";

        var ctx = GivenHttpContextWith([new(ClaimType.CdpClaims, json)]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        svc.HasApplicationPermission(orgId, "fts-app", "submit:notice").Should().BeFalse();
    }

    [Fact]
    public void HasApplicationPermission_ShouldReturnFalse_WhenNoCdpClaimsClaim()
    {
        var ctx = GivenHttpContextWith([]);
        var svc = new ClaimService(ctx.Object, mockOrgRepo.Object);

        svc.HasApplicationPermission(Guid.NewGuid(), "any-app", "any:perm").Should().BeFalse();
    }

    private static Mock<IHttpContextAccessor> GivenHttpContextWith(List<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        return httpContextAccessor;
    }
}