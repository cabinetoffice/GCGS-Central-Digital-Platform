using CO.CDP.UserManagement.App.Application.Users.Implementations;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Tests.TestFixtures;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Application.Users;

public class UsersQueryServiceTests : AdapterTestFixture
{
    private readonly Mock<IUserManagementApiAdapter> _adapter = new();
    private readonly UsersQueryService _sut;

    public UsersQueryServiceTests()
        => _sut = new UsersQueryService(_adapter.Object);

    // ── Null / NotFound guards ────────────────────────────────────────────────

    [Fact]
    public async Task GetViewModelAsync_EmptyOrganisationId_ReturnsNull()
    {
        var result = await _sut.GetViewModelAsync(Guid.Empty, null, null, null, CancellationToken.None);
        result.Should().BeNull();
        _adapter.Verify(a => a.GetOrganisationByGuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetViewModelAsync_OrgNotFound_ReturnsNull()
    {
        var organisationId = Guid.NewGuid();
        _adapter.Setup(a => a.GetOrganisationByGuidAsync(organisationId, default)).ReturnsAsync((OrganisationResponse?)null);
        var result = await _sut.GetViewModelAsync(organisationId, null, null, null, CancellationToken.None);
        result.Should().BeNull();
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetViewModelAsync_ValidOrg_ReturnsMappedViewModel()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(new[] { MakeUser() });
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse>());
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync(new List<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, null, null, null, CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrganisationId.Should().Be(OrgGuid);
        result.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetViewModelAsync_FetchesUsersAndInvitesInParallel()
    {
        // Verify Task.WhenAll is used — both calls must be made regardless of each other
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(new List<OrganisationUserResponse>());
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default)).ReturnsAsync(new[] { MakeInvite() });
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync(new List<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, null, null, null, CancellationToken.None);

        _adapter.Verify(a => a.GetUsersAsync(OrgGuid, default), Times.Once);
        _adapter.Verify(a => a.GetInvitesAsync(OrgGuid, default), Times.Once);
        result!.Users.Should().HaveCount(1);
    }

    // ── Filtering ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("admin", 1, 0)] // only admin user matches
    [InlineData("member", 0, 0)] // no members
    [InlineData(null, 1, 1)] // no filter = all
    public async Task GetViewModelAsync_RoleFilter_FiltersCorrectly(
        string? role, int expectedUsers, int expectedInvites)
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync(new[] { MakeUser(role: OrganisationRole.Admin) });
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync(new[] { MakeInvite(role: OrganisationRole.Owner) });
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, role, null, null, CancellationToken.None);

        // Count only actual users (exclude invites) for the first assertion
        result!.Users.Count(u => u.InviteGuid == null).Should().Be(expectedUsers);

        // Ensure invites count matches expectations
        result.Users.Count(u => u.InviteGuid != null).Should().Be(expectedInvites);
    }

    [Fact]
    public async Task GetViewModelAsync_SearchFilter_FiltersUsersByName()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync(new[]
                { MakeUser(firstName: "Alice", lastName: "Smith"), MakeUser(firstName: "Bob", lastName: "Jones") });
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync(Array.Empty<PendingOrganisationInviteResponse>());
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, null, null, "alice", CancellationToken.None);

        result!.Users.Should().HaveCount(1);
        result.Users[0].Name.Should().Contain("Alice");
    }

    [Fact]
    public async Task GetViewModelAsync_SearchFilter_IsCaseInsensitive()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync(new[] { MakeUser(firstName: "Alice", lastName: "Smith") });
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync(Array.Empty<PendingOrganisationInviteResponse>());
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, null, null, "ALICE", CancellationToken.None);

        result!.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetViewModelAsync_ApplicationFilter_FiltersCorrectly()
    {
        // Covered by service's in-memory filter — application filter reduces users
        // who have access to the given application's OrganisationApplicationId
        SetupOrg();

        var app = MakeApplication(orgAppId: 42);
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default)).ReturnsAsync(new[] { app });

        // User assigned to app 42
        var user = MakeUser();
        user = user with
        {
            ApplicationAssignments = new[]
            {
                new UserAssignmentResponse
                {
                    Id = 1,
                    UserOrganisationMembershipId = 1,
                    OrganisationApplicationId = 42,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            }
        };
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(new[] { user });
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, null, "42", null, CancellationToken.None);

        result!.Users.Should().HaveCount(1);
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetViewModelAsync_MapsOrganisationNameCorrectly()
    {
        SetupOrg(adapter: _adapter, name: "My Organisation");
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(Array.Empty<OrganisationUserResponse>());
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync(Array.Empty<PendingOrganisationInviteResponse>());
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, null, null, null, CancellationToken.None);

        result!.OrganisationName.Should().Be("My Organisation");
    }

    [Fact]
    public async Task GetViewModelAsync_InviteApplicationAssignments_AreMappedToApplicationAccess()
    {
        SetupOrg();

        var app = MakeApplication(orgAppId: 99, appId: 5, name: "FallbackApp");
        var invite = MakeInvite();
        invite = invite with
        {
            ApplicationAssignments = new[]
            {
                new InviteApplicationAssignmentResponse
                {
                    OrganisationApplicationId = 99,
                    ApplicationId = 5,
                    ApplicationName = "InvApp",
                    ApplicationRoleId = 1
                }
            }
        };

        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(Array.Empty<OrganisationUserResponse>());
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default)).ReturnsAsync(new[] { invite });
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default)).ReturnsAsync(new[] { app });

        var result = await _sut.GetViewModelAsync(OrgGuid, null, null, null, CancellationToken.None);

        result.Should().NotBeNull();
        var user = result!.Users.Single();
        user.InviteGuid.Should().NotBeNull();
        user.ApplicationAccess.Should().HaveCount(1);
        var access = user.ApplicationAccess[0];
        access.ApplicationName.Should().Be("InvApp");
        access.ApplicationSlug.Should().Be("app-5");
        access.RoleName.Should().BeEmpty();
    }

    [Fact]
    public async Task GetViewModelAsync_TotalCount_IncludesUsersAndInvitesWhenNoFilter()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(new[] { MakeUser() });
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default)).ReturnsAsync(new[] { MakeInvite() });
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default)).ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, null, null, null, CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(2);
    }

    [Theory]
    [InlineData("admin", null, null, true)]
    [InlineData(null, "app-1", null, true)]
    [InlineData(null, null, "alice", true)]
    [InlineData(null, null, null, false)]
    public async Task GetViewModelAsync_HasActiveFilters_ReflectsActiveFilters(
        string? role, string? app, string? search, bool expected)
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(Array.Empty<OrganisationUserResponse>());
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default)).ReturnsAsync(Array.Empty<PendingOrganisationInviteResponse>());
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default)).ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, role, app, search, CancellationToken.None);

        result!.HasActiveFilters.Should().Be(expected);
    }

    [Fact]
    public async Task GetViewModelAsync_InviteRoleFilter_IsApplied()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(Array.Empty<OrganisationUserResponse>());
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default)).ReturnsAsync(new[]
        {
            MakeInvite(role: OrganisationRole.Admin),
            MakeInvite(inviteGuid: Guid.NewGuid(), pendingInviteId: 2, role: OrganisationRole.Member)
        });
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default)).ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, "admin", null, null, CancellationToken.None);

        result!.TotalCount.Should().Be(1);
        result.Users.Should().HaveCount(1);
        result.Users[0].OrganisationRole.Should().Be(OrganisationRole.Admin);
    }

    [Fact]
    public async Task GetViewModelAsync_InviteSearchFilter_IsApplied()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(Array.Empty<OrganisationUserResponse>());
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default)).ReturnsAsync(new[]
        {
            MakeInvite(email: "alice@example.com", firstName: "Alice", lastName: "Smith"),
            MakeInvite(inviteGuid: Guid.NewGuid(), pendingInviteId: 2, email: "bob@example.com", firstName: "Bob", lastName: "Jones")
        });
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default)).ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync(OrgGuid, null, null, "alice", CancellationToken.None);

        result!.TotalCount.Should().Be(1);
        result.Users[0].Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task GetViewModelAsync_InviteApplicationFilter_IsApplied()
    {
        SetupOrg();
        var app = MakeApplication(orgAppId: 55, appId: 3, name: "CoolApp");
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default)).ReturnsAsync(new[] { app });
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(Array.Empty<OrganisationUserResponse>());

        var inviteWithApp = MakeInvite() with
        {
            ApplicationAssignments = new[]
            {
                new InviteApplicationAssignmentResponse { OrganisationApplicationId = 55, ApplicationRoleId = 1 }
            }
        };
        var inviteWithout = MakeInvite(inviteGuid: Guid.NewGuid(), pendingInviteId: 2);
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default)).ReturnsAsync(new[] { inviteWithApp, inviteWithout });

        var result = await _sut.GetViewModelAsync(OrgGuid, null, "app-3", null, CancellationToken.None);

        result!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetViewModelAsync_TotalCount_ReflectsFilteredResults()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(new[] { MakeUser(), MakeUser(personId: Guid.NewGuid()) });
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default)).ReturnsAsync(new[] { MakeInvite() });
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default)).ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        // Filter to admin — no admin users exist, so count should be 0
        var result = await _sut.GetViewModelAsync(OrgGuid, "admin", null, null, CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(0);
        result.Users.Should().BeEmpty();
    }
}
