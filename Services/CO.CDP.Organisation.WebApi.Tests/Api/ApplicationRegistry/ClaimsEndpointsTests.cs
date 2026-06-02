using System.Net;
using System.Net.Http.Json;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api.ApplicationRegistry;

/// <summary>
/// Tests for <c>ClaimsEndpoints</c> (Phase 3B — Claims Service).
/// GET /api/claims/{userPrincipalId}            → PlatformAdmin only
/// GET /api/claims/{userPrincipalId}/organisations/{orgId} → OrgMember
/// </summary>
public class ClaimsEndpointsTests
{
    private readonly Mock<IUseCase<string, ClaimsTree>> _getClaimsTreeUseCase = new();

    // ── GET /api/claims/{userPrincipalId} ─────────────────────────────────

    [Fact]
    public async Task GetClaimsTree_Returns_Ok_For_KnownUser()
    {
        const string userUrn = "urn:fdc:gov.uk:2022:abc123";
        var tree = GivenClaimsTree(userUrn, []);
        _getClaimsTreeUseCase.Setup(uc => uc.Execute(userUrn)).ReturnsAsync(tree);

        var response = await PlatformAdminClient().GetAsync($"/api/claims/{Uri.EscapeDataString(userUrn)}");

        response.Should().HaveStatusCode(OK);
        var result = await response.Content.ReadFromJsonAsync<ClaimsTree>();
        result!.UserPrincipalId.Should().Be(userUrn);
    }

    [Theory]
    [InlineData(OK,        true)]   // PlatformAdmin
    [InlineData(Forbidden, false)]  // No matching auth
    public async Task GetClaimsTree_Authorization(HttpStatusCode expected, bool platformAdmin)
    {
        const string userUrn = "urn:test";
        _getClaimsTreeUseCase.Setup(uc => uc.Execute(It.IsAny<string>()))
            .ReturnsAsync(GivenClaimsTree(userUrn, []));

        var client   = platformAdmin ? PlatformAdminClient() : UnauthorizedClient();
        var response = await client.GetAsync($"/api/claims/{Uri.EscapeDataString(userUrn)}");
        response.StatusCode.Should().Be(expected);
    }

    // ── GET /api/claims/{userPrincipalId}/organisations/{orgId} ──────────

    [Fact]
    public async Task GetOrgClaims_Returns_Ok_For_OrgMember()
    {
        var orgId   = Guid.NewGuid();
        const string userUrn = "urn:fdc:gov.uk:2022:xyz789";

        var orgClaims = new OrganisationClaims(orgId, "Test Org", "Member", []);
        var tree      = GivenClaimsTree(userUrn, [orgClaims]);
        _getClaimsTreeUseCase.Setup(uc => uc.Execute(userUrn)).ReturnsAsync(tree);

        var response = await OrgMemberClient(orgId)
            .GetAsync($"/api/claims/{Uri.EscapeDataString(userUrn)}/organisations/{orgId}");

        response.Should().HaveStatusCode(OK);
        var result = await response.Content.ReadFromJsonAsync<OrganisationClaims>();
        result!.OrganisationId.Should().Be(orgId);
        result.OrganisationName.Should().Be("Test Org");
    }

    [Fact]
    public async Task GetOrgClaims_Returns_NotFound_WhenOrgNotInTree()
    {
        var orgId   = Guid.NewGuid();
        const string userUrn = "urn:test";
        // Tree has no organisations
        _getClaimsTreeUseCase.Setup(uc => uc.Execute(userUrn))
            .ReturnsAsync(GivenClaimsTree(userUrn, []));

        var response = await OrgMemberClient(orgId)
            .GetAsync($"/api/claims/{Uri.EscapeDataString(userUrn)}/organisations/{orgId}");

        response.Should().HaveStatusCode(NotFound);
    }

    [Theory]
    [InlineData(OK,        "Member")]
    [InlineData(OK,        "Admin")]
    [InlineData(Forbidden, null)]   // No org role
    public async Task GetOrgClaims_Authorization(HttpStatusCode expected, string? orgRole)
    {
        var orgId   = Guid.NewGuid();
        const string userUrn = "urn:test";

        var orgClaims = new OrganisationClaims(orgId, "Org", "Member", []);
        _getClaimsTreeUseCase.Setup(uc => uc.Execute(It.IsAny<string>()))
            .ReturnsAsync(GivenClaimsTree(userUrn, [orgClaims]));

        HttpClient client = orgRole switch
        {
            "Admin"  => AppRegistryTestFactory.OrgAdmin(orgId,   RegisterUseCase),
            "Member" => AppRegistryTestFactory.OrgMember(orgId,  RegisterUseCase),
            _        => AppRegistryTestFactory.Unauthorized(RegisterUseCase)
        };

        var response = await client
            .GetAsync($"/api/claims/{Uri.EscapeDataString(userUrn)}/organisations/{orgId}");
        response.StatusCode.Should().Be(expected);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void RegisterUseCase(IServiceCollection services)
        => services.AddScoped(_ => _getClaimsTreeUseCase.Object);

    private HttpClient PlatformAdminClient() =>
        AppRegistryTestFactory.PlatformAdmin(RegisterUseCase);

    private HttpClient OrgMemberClient(Guid orgId) =>
        AppRegistryTestFactory.OrgMember(orgId, RegisterUseCase);

    private HttpClient UnauthorizedClient() =>
        AppRegistryTestFactory.Unauthorized(RegisterUseCase);

    private static ClaimsTree GivenClaimsTree(string userPrincipalId, IEnumerable<OrganisationClaims> orgs)
        => new(userPrincipalId, orgs.ToList());
}
