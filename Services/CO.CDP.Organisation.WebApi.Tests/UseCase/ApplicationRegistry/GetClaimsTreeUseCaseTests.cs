using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Claims;
using FluentAssertions;
using Moq;
using Application = CO.CDP.ApplicationRegistry.Persistence.Entities.Application;
using AppPerm    = CO.CDP.ApplicationRegistry.Persistence.Entities.ApplicationPermission;
using AppRole    = CO.CDP.ApplicationRegistry.Persistence.Entities.ApplicationRole;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase.ApplicationRegistry;

/// <summary>
/// Unit tests for <c>GetClaimsTreeUseCase</c> (Phase 3B — Claims Service).
/// Verifies the cross-collection claim hydration:
///   orgs → memberships → enabled apps → user assignments → roles → permissions.
/// </summary>
public class GetClaimsTreeUseCaseTests
{
    private readonly Mock<IOrganisationRepository>    _orgRepo        = new();
    private readonly Mock<IUserAssignmentRepository>  _assignmentRepo = new();

    private GetClaimsTreeUseCase UseCase =>
        new(_orgRepo.Object, _assignmentRepo.Object);

    // ── No memberships ─────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_Returns_EmptyTree_WhenUserHasNoMemberships()
    {
        const string userUrn = "urn:test:user";
        var org = GivenOrg("ACME");

        _orgRepo.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync([org]);
        _orgRepo.Setup(r => r.GetMemberAsync(org.Id, userUrn)).ReturnsAsync((UserOrganisationMembership?)null);

        var result = await UseCase.Execute(userUrn);

        result.UserPrincipalId.Should().Be(userUrn);
        result.Organisations.Should().BeEmpty();
    }

    // ── Member with no app assignments ─────────────────────────────────────

    [Fact]
    public async Task Execute_Returns_OrgClaims_WithNoApps_WhenUserHasNoAssignments()
    {
        const string userUrn = "urn:test:user";
        var org    = GivenOrg("ACME");
        var member = GivenMembership(org.Id, userUrn, "Admin");
        var app    = GivenApp("FTS");

        _orgRepo.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync([org]);
        _orgRepo.Setup(r => r.GetMemberAsync(org.Id, userUrn)).ReturnsAsync(member);
        _orgRepo.Setup(r => r.GetOrganisationApplicationsAsync(org.Id))
            .ReturnsAsync([GivenOrgApp(org, app)]);
        _assignmentRepo.Setup(r => r.GetAssignmentAsync(org.Id, app.Id, userUrn))
            .ReturnsAsync((UserApplicationAssignment?)null);

        var result = await UseCase.Execute(userUrn);

        result.Organisations.Should().HaveCount(1);
        result.Organisations.First().OrganisationRole.Should().Be("Admin");
        result.Organisations.First().Applications.Should().BeEmpty();
    }

    // ── Member with assignment and roles ───────────────────────────────────

    [Fact]
    public async Task Execute_Returns_Correct_Roles_For_Assigned_User()
    {
        const string userUrn = "urn:test:user";
        var org    = GivenOrg("ACME");
        var app    = GivenApp("FTS", "fts-app");
        var member = GivenMembership(org.Id, userUrn, "Member");

        var (role1, _) = GivenRole(app.Id, "Buyer");
        var assignment = GivenAssignment(org.Id, app.Id, userUrn, [role1]);

        _orgRepo.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync([org]);
        _orgRepo.Setup(r => r.GetMemberAsync(org.Id, userUrn)).ReturnsAsync(member);
        _orgRepo.Setup(r => r.GetOrganisationApplicationsAsync(org.Id))
            .ReturnsAsync([GivenOrgApp(org, app)]);
        _assignmentRepo.Setup(r => r.GetAssignmentAsync(org.Id, app.Id, userUrn))
            .ReturnsAsync(assignment);

        var result = await UseCase.Execute(userUrn);

        var orgClaims = result.Organisations.Single();
        orgClaims.OrganisationId.Should().Be(org.Id);
        orgClaims.OrganisationRole.Should().Be("Member");

        var appClaims = orgClaims.Applications.Single();
        appClaims.ApplicationId.Should().Be(app.Id);
        appClaims.ApplicationName.Should().Be("FTS");
        appClaims.Roles.Should().ContainSingle().Which.Should().Be("Buyer");
    }

    // ── Permissions are flattened from all roles ───────────────────────────

    [Fact]
    public async Task Execute_Flattens_Permissions_From_All_Roles()
    {
        const string userUrn = "urn:test:user";
        var org  = GivenOrg("ACME");
        var app  = GivenApp("FTS");
        var perm1 = GivenPermission(app.Id, "submit:notice");
        var perm2 = GivenPermission(app.Id, "read:reports");

        var (role1, _) = GivenRole(app.Id, "Buyer",    [perm1]);
        var (role2, _) = GivenRole(app.Id, "Reviewer", [perm2]);
        var assignment = GivenAssignment(org.Id, app.Id, userUrn, [role1, role2]);

        _orgRepo.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync([org]);
        _orgRepo.Setup(r => r.GetMemberAsync(org.Id, userUrn))
            .ReturnsAsync(GivenMembership(org.Id, userUrn, "Admin"));
        _orgRepo.Setup(r => r.GetOrganisationApplicationsAsync(org.Id))
            .ReturnsAsync([GivenOrgApp(org, app)]);
        _assignmentRepo.Setup(r => r.GetAssignmentAsync(org.Id, app.Id, userUrn))
            .ReturnsAsync(assignment);

        var result = await UseCase.Execute(userUrn);

        var appClaims = result.Organisations.Single().Applications.Single();
        appClaims.Roles.Should().BeEquivalentTo(["Buyer", "Reviewer"]);
        appClaims.Permissions.Should().BeEquivalentTo(["submit:notice", "read:reports"]);
    }

    // ── Duplicate permissions de-duplicated ───────────────────────────────

    [Fact]
    public async Task Execute_Deduplicates_Permissions_Shared_Across_Roles()
    {
        const string userUrn = "urn:test:user";
        var org  = GivenOrg("Org");
        var app  = GivenApp("App");
        var sharedPerm = GivenPermission(app.Id, "read:data");

        var (role1, _) = GivenRole(app.Id, "RoleA", [sharedPerm]);
        var (role2, _) = GivenRole(app.Id, "RoleB", [sharedPerm]); // same permission
        var assignment = GivenAssignment(org.Id, app.Id, userUrn, [role1, role2]);

        _orgRepo.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync([org]);
        _orgRepo.Setup(r => r.GetMemberAsync(org.Id, userUrn))
            .ReturnsAsync(GivenMembership(org.Id, userUrn, "Member"));
        _orgRepo.Setup(r => r.GetOrganisationApplicationsAsync(org.Id))
            .ReturnsAsync([GivenOrgApp(org, app)]);
        _assignmentRepo.Setup(r => r.GetAssignmentAsync(org.Id, app.Id, userUrn))
            .ReturnsAsync(assignment);

        var result = await UseCase.Execute(userUrn);

        var appClaims = result.Organisations.Single().Applications.Single();
        appClaims.Permissions.Should().HaveCount(1).And.Contain("read:data");
    }

    // ── Multiple orgs — only orgs with membership included ────────────────

    [Fact]
    public async Task Execute_Includes_Only_Orgs_With_Membership()
    {
        const string userUrn = "urn:test:user";
        var org1 = GivenOrg("Org1");
        var org2 = GivenOrg("Org2"); // user is NOT a member here

        _orgRepo.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync([org1, org2]);
        _orgRepo.Setup(r => r.GetMemberAsync(org1.Id, userUrn))
            .ReturnsAsync(GivenMembership(org1.Id, userUrn, "Admin"));
        _orgRepo.Setup(r => r.GetMemberAsync(org2.Id, userUrn)).ReturnsAsync((UserOrganisationMembership?)null);
        _orgRepo.Setup(r => r.GetOrganisationApplicationsAsync(org1.Id)).ReturnsAsync([]);

        var result = await UseCase.Execute(userUrn);

        result.Organisations.Should().HaveCount(1);
        result.Organisations.Single().OrganisationName.Should().Be("Org1");
    }

    // ── Builder helpers ────────────────────────────────────────────────────

    private static CO.CDP.ApplicationRegistry.Persistence.Entities.Organisation GivenOrg(string name) =>
        new() { Id = Guid.NewGuid(), Name = name, Slug = name.ToLower() };

    private static Application GivenApp(string name, string? clientId = null) =>
        new() { Id = Guid.NewGuid(), Name = name, ClientId = clientId ?? $"{name.ToLower()}-app" };

    private static AppPerm GivenPermission(Guid appId, string name) =>
        new() { Id = Guid.NewGuid(), ApplicationId = appId, Name = name };

    private static (AppRole role, IList<RolePermission> rolePerms) GivenRole(
        Guid appId,
        string name,
        IEnumerable<AppPerm>? permissions = null)
    {
        var roleId = Guid.NewGuid();
        var perms  = (permissions ?? []).ToList();
        var rolePerms = perms.Select(p => new RolePermission
        {
            RoleId       = roleId,
            PermissionId = p.Id,
            Permission   = p
        }).ToList();

        var role = new AppRole
        {
            Id = roleId, ApplicationId = appId, Name = name,
            RolePermissions = rolePerms
        };

        return (role, rolePerms);
    }

    private static UserOrganisationMembership GivenMembership(
        Guid orgId, string userUrn, string role) =>
        new() { Id = Guid.NewGuid(), OrganisationId = orgId, UserPrincipalId = userUrn, OrganisationRole = role };

    private static OrganisationApplication GivenOrgApp(
        CO.CDP.ApplicationRegistry.Persistence.Entities.Organisation org,
        Application app) =>
        new() { OrganisationId = org.Id, ApplicationId = app.Id, Application = app, Organisation = org, EnabledBy = "system" };

    private static UserApplicationAssignment GivenAssignment(
        Guid orgId, Guid appId, string userUrn,
        IEnumerable<AppRole> roles)
    {
        var assignment = new UserApplicationAssignment
        {
            Id = Guid.NewGuid(), OrganisationId = orgId, ApplicationId = appId,
            UserPrincipalId = userUrn, AssignedBy = "system"
        };

        assignment.RoleAssignments = roles.Select(r => new UserRoleAssignment
        {
            UserApplicationAssignmentId = assignment.Id,
            RoleId = r.Id,
            Role   = r,
            UserApplicationAssignment = assignment
        }).ToList();

        return assignment;
    }
}
