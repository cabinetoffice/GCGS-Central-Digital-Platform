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
    public async Task GetViewModelAsync_NullSlug_ReturnsNull()
    {
        var result = await _sut.GetViewModelAsync(null, null, null, null, CancellationToken.None);
        result.Should().BeNull();
        _adapter.Verify(a => a.GetOrganisationBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetViewModelAsync_OrgNotFound_ReturnsNull()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("unknown", default)).ReturnsAsync((OrganisationResponse?)null);
        var result = await _sut.GetViewModelAsync("unknown", null, null, null, CancellationToken.None);
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

        var result = await _sut.GetViewModelAsync("test-org", null, null, null, CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrganisationSlug.Should().Be("test-org");
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

        var result = await _sut.GetViewModelAsync("test-org", null, null, null, CancellationToken.None);

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

        var result = await _sut.GetViewModelAsync("test-org", role, null, null, CancellationToken.None);

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

        var result = await _sut.GetViewModelAsync("test-org", null, null, "alice", CancellationToken.None);

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

        var result = await _sut.GetViewModelAsync("test-org", null, null, "ALICE", CancellationToken.None);

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

        var result = await _sut.GetViewModelAsync("test-org", null, "42", null, CancellationToken.None);

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

        var result = await _sut.GetViewModelAsync("test-org", null, null, null, CancellationToken.None);

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

        var result = await _sut.GetViewModelAsync("test-org", null, null, null, CancellationToken.None);

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
    public async Task GetViewModelAsync_TotalCount_IncludesInvites()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default)).ReturnsAsync(new[] { MakeUser() });
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default)).ReturnsAsync(new[] { MakeInvite() });
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default)).ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync("test-org", null, null, null, CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(2);
    }
}
