using System.Net;
using System.Net.Http.Json;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Organisation;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api.ApplicationRegistry;

/// <summary>
/// Tests for <c>OrganisationEndpoints</c> in the ApplicationRegistry subsystem
/// (Phase 2C — Org-App Linkage).
/// </summary>
public class AppRegistryOrganisationEndpointsTests
{
    private readonly Mock<IUseCase<GetOrganisationsQuery,    IEnumerable<OrganisationDto>>> _getOrgsUseCase  = new();
    private readonly Mock<IUseCase<CreateOrganisation,       OrganisationDto>>               _createOrgUseCase = new();
    private readonly Mock<IUseCase<Guid,                     OrganisationDto?>>              _getOrgUseCase   = new();
    private readonly Mock<IUseCase<(Guid, UpdateOrganisation), bool>>                        _updateOrgUseCase = new();
    private readonly Mock<IUseCase<Guid, IEnumerable<MemberDto>>>                            _getMembersUseCase = new();
    private readonly Mock<IUseCase<(Guid, AddMember), bool>>                                 _addMemberUseCase = new();
    private readonly Mock<IOrganisationRepository>                                           _orgRepo          = new();

    // ── GET /api/organisations ─────────────────────────────────────────────

    [Fact]
    public async Task GetOrganisations_Returns_Ok_With_List()
    {
        _getOrgsUseCase.Setup(uc => uc.Execute(It.IsAny<GetOrganisationsQuery>()))
            .ReturnsAsync([GivenOrgDto("ACME")]);

        var response = await PlatformAdminClient().GetAsync("/api/organisations");

        response.Should().HaveStatusCode(OK);
        var result = await response.Content.ReadFromJsonAsync<OrganisationDto[]>();
        result.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(OK,        true)]
    [InlineData(Forbidden, false)]
    public async Task GetOrganisations_Authorization(HttpStatusCode expected, bool platformAdmin)
    {
        _getOrgsUseCase.Setup(uc => uc.Execute(It.IsAny<GetOrganisationsQuery>())).ReturnsAsync([]);

        var client   = platformAdmin ? PlatformAdminClient() : UnauthorizedClient();
        var response = await client.GetAsync("/api/organisations");
        response.StatusCode.Should().Be(expected);
    }

    // ── POST /api/organisations ────────────────────────────────────────────

    [Fact]
    public async Task CreateOrganisation_Returns_Created()
    {
        var command = new CreateOrganisation("New Org", "Authority", null);
        var dto     = GivenOrgDto("New Org");
        _createOrgUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(dto);

        var response = await PlatformAdminClient().PostAsJsonAsync("/api/organisations", command);

        response.Should().HaveStatusCode(Created);
        response.Headers.Location?.ToString().Should().Contain(dto.Id.ToString());
    }

    // ── GET /api/organisations/{orgId} ────────────────────────────────────

    [Fact]
    public async Task GetOrganisation_Returns_Ok_WhenFound()
    {
        var orgId = Guid.NewGuid();
        var dto   = GivenOrgDto("ACME", orgId);
        _getOrgUseCase.Setup(uc => uc.Execute(orgId)).ReturnsAsync(dto);

        var response = await OrgMemberClient(orgId).GetAsync($"/api/organisations/{orgId}");

        response.Should().HaveStatusCode(OK);
        var result = await response.Content.ReadFromJsonAsync<OrganisationDto>();
        result!.Id.Should().Be(orgId);
    }

    [Fact]
    public async Task GetOrganisation_Returns_NotFound_WhenMissing()
    {
        var orgId = Guid.NewGuid();
        _getOrgUseCase.Setup(uc => uc.Execute(orgId)).ReturnsAsync((OrganisationDto?)null);

        var response = await OrgMemberClient(orgId).GetAsync($"/api/organisations/{orgId}");
        response.Should().HaveStatusCode(NotFound);
    }

    [Theory]
    [InlineData(OK,        "Member")]
    [InlineData(OK,        "Admin")]
    [InlineData(Forbidden, null)]   // no org role
    public async Task GetOrganisation_Authorization(HttpStatusCode expected, string? orgRole)
    {
        var orgId = Guid.NewGuid();
        _getOrgUseCase.Setup(uc => uc.Execute(orgId)).ReturnsAsync(GivenOrgDto("O", orgId));

        HttpClient client = orgRole switch
        {
            "Admin"  => AppRegistryTestFactory.OrgAdmin(orgId,  RegisterAllServices),
            "Member" => AppRegistryTestFactory.OrgMember(orgId, RegisterAllServices),
            _        => AppRegistryTestFactory.Unauthorized(RegisterAllServices)
        };

        var response = await client.GetAsync($"/api/organisations/{orgId}");
        response.StatusCode.Should().Be(expected);
    }

    // ── GET /api/organisations/{orgId}/members ─────────────────────────────

    [Fact]
    public async Task GetMembers_Returns_Ok_For_OrgAdmin()
    {
        var orgId = Guid.NewGuid();
        var member = new MemberDto(Guid.NewGuid(), "urn:user", "Admin", DateTimeOffset.UtcNow, true);
        _getMembersUseCase.Setup(uc => uc.Execute(orgId)).ReturnsAsync([member]);

        var response = await OrgAdminClient(orgId).GetAsync($"/api/organisations/{orgId}/members");

        response.Should().HaveStatusCode(OK);
        var result = await response.Content.ReadFromJsonAsync<MemberDto[]>();
        result.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(OK,        true)]
    [InlineData(Forbidden, false)]
    public async Task GetMembers_Authorization(HttpStatusCode expected, bool orgAdmin)
    {
        var orgId = Guid.NewGuid();
        _getMembersUseCase.Setup(uc => uc.Execute(orgId)).ReturnsAsync([]);

        var client   = orgAdmin ? OrgAdminClient(orgId) : UnauthorizedClient();
        var response = await client.GetAsync($"/api/organisations/{orgId}/members");
        response.StatusCode.Should().Be(expected);
    }

    // ── POST /api/organisations/{orgId}/members ────────────────────────────

    [Fact]
    public async Task AddMember_Returns_Created_For_OrgAdmin()
    {
        var orgId   = Guid.NewGuid();
        var command = new AddMember("urn:new:member", "Member");
        _addMemberUseCase.Setup(uc => uc.Execute(It.IsAny<(Guid, AddMember)>())).ReturnsAsync(true);

        var response = await OrgAdminClient(orgId)
            .PostAsJsonAsync($"/api/organisations/{orgId}/members", command);

        response.Should().HaveStatusCode(Created);
    }

    [Theory]
    [InlineData(Created,   true)]
    [InlineData(Forbidden, false)]
    public async Task AddMember_Authorization(HttpStatusCode expected, bool orgAdmin)
    {
        var orgId = Guid.NewGuid();
        _addMemberUseCase.Setup(uc => uc.Execute(It.IsAny<(Guid, AddMember)>())).ReturnsAsync(true);

        var client   = orgAdmin ? OrgAdminClient(orgId) : UnauthorizedClient();
        var response = await client.PostAsJsonAsync(
            $"/api/organisations/{orgId}/members",
            new AddMember("urn:u", "Member"));
        response.StatusCode.Should().Be(expected);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void RegisterAllServices(IServiceCollection services)
    {
        services.AddScoped(_ => _getOrgsUseCase.Object);
        services.AddScoped(_ => _createOrgUseCase.Object);
        services.AddScoped(_ => _getOrgUseCase.Object);
        services.AddScoped(_ => _updateOrgUseCase.Object);
        services.AddScoped(_ => _getMembersUseCase.Object);
        services.AddScoped(_ => _addMemberUseCase.Object);
        services.AddScoped<IOrganisationRepository>(_ => _orgRepo.Object);
    }

    private HttpClient PlatformAdminClient() =>
        AppRegistryTestFactory.PlatformAdmin(RegisterAllServices);

    private HttpClient OrgAdminClient(Guid orgId) =>
        AppRegistryTestFactory.OrgAdmin(orgId, RegisterAllServices);

    private HttpClient OrgMemberClient(Guid orgId) =>
        AppRegistryTestFactory.OrgMember(orgId, RegisterAllServices);

    private HttpClient UnauthorizedClient() =>
        AppRegistryTestFactory.Unauthorized(RegisterAllServices);

    private static OrganisationDto GivenOrgDto(string name, Guid? id = null) =>
        new(id ?? Guid.NewGuid(), name, name.ToLower(), "Authority", null, true,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
