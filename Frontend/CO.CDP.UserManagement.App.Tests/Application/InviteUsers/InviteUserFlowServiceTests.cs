using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Application.InviteUsers.Implementations;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.App.Tests.TestFixtures;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Application.InviteUsers;

public class InviteUserFlowServiceTests : AdapterTestFixture
{
    private readonly Mock<IUserManagementApiAdapter> _adapter = new();
    private readonly InviteUserFlowService _sut;

    public InviteUserFlowServiceTests()
        => _sut = new InviteUserFlowService(_adapter.Object);

    private void SetupOrg(string name = "Test Org") => base.SetupOrg(_adapter, name);

    // ── GetAddViewModelAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAddViewModelAsync_OrgNotFound_ReturnsNull()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.GetViewModelAsync("slug", null, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddViewModelAsync_NullInput_ReturnsEmptyViewModel()
    {
        SetupOrg();

        var result = await _sut.GetViewModelAsync("test-org", null, CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrganisationName.Should().Be("Test Org");
        result.Email.Should().BeNull();
        result.FirstName.Should().BeNull();
    }

    [Fact]
    public async Task GetAddViewModelAsync_WithInput_PrePopulatesFields()
    {
        SetupOrg();
        var input = new InviteUserViewModel
            { Email = "test@example.com", FirstName = "Jane", LastName = "Doe" };

        var result = await _sut.GetViewModelAsync("test-org", input, CancellationToken.None);

        result!.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Doe");
    }

    // ── IsEmailAlreadyInOrganisationAsync ─────────────────────────────────────

    [Fact]
    public async Task IsEmailAlreadyInOrganisation_OrgNotFound_ReturnsFalse()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.IsEmailAlreadyInOrganisationAsync(
            "slug", "test@example.com", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEmailAlreadyInOrganisation_EmailMatchesExistingUser_ReturnsTrue()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync([MakeUser(email: "test@example.com")]);
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync([]);

        var result = await _sut.IsEmailAlreadyInOrganisationAsync(
            "test-org", "test@example.com", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailAlreadyInOrganisation_EmailMatchesPendingInvite_ReturnsTrue()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync([]);
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync([MakeInvite(email: "test@example.com")]);

        var result = await _sut.IsEmailAlreadyInOrganisationAsync(
            "test-org", "test@example.com", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailAlreadyInOrganisation_EmailCheckIsCaseInsensitive()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync([MakeUser(email: "TEST@EXAMPLE.COM")]);
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync([]);

        var result = await _sut.IsEmailAlreadyInOrganisationAsync(
            "test-org", "test@example.com", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailAlreadyInOrganisation_NoMatch_ReturnsFalse()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync([MakeUser(email: "other@example.com")]);
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync([]);

        var result = await _sut.IsEmailAlreadyInOrganisationAsync(
            "test-org", "test@example.com", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEmailAlreadyInOrganisation_FetchesUsersAndInvitesInParallel()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync([]);
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync([]);

        await _sut.IsEmailAlreadyInOrganisationAsync("test-org", "x@x.com", CancellationToken.None);

        _adapter.Verify(a => a.GetUsersAsync(OrgGuid, default), Times.Once);
        _adapter.Verify(a => a.GetInvitesAsync(OrgGuid, default), Times.Once);
    }

    // ── GetApplicationRolesStepAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetApplicationRolesStepAsync_OrgNotFound_ReturnsNull()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.GetApplicationRolesStepAsync(
            "slug", MakeState(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetApplicationRolesStepAsync_FetchesRolesForEachApplicationInParallel()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([MakeApplication(appId: 1), MakeApplication(appId: 2)]);
        _adapter.Setup(a => a.GetApplicationRolesAsync(OrgId, 1, default))
            .ReturnsAsync([MakeRole(id: 10, name: "Viewer")]);
        _adapter.Setup(a => a.GetApplicationRolesAsync(OrgId, 2, default))
            .ReturnsAsync([MakeRole(id: 20, name: "Editor")]);

        var result = await _sut.GetApplicationRolesStepAsync(
            "test-org", MakeState(), CancellationToken.None);

        result!.Applications.Should().HaveCount(2);
        result.Applications[0].Roles.Should().ContainSingle(r => r.Name == "Viewer");
        result.Applications[1].Roles.Should().ContainSingle(r => r.Name == "Editor");
    }

    [Fact]
    public async Task GetApplicationRolesStepAsync_PopulatesStatePersonDetails()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([]);
        var state = MakeState(firstName: "Jane", lastName: "Doe");

        var result = await _sut.GetApplicationRolesStepAsync("test-org", state, CancellationToken.None);

        result!.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetApplicationRolesStepAsync_NoSelectionApplied_ControllerResponsibility()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([MakeApplication(orgAppId: 5)]);
        _adapter.Setup(a => a.GetApplicationRolesAsync(OrgId, It.IsAny<int>(), default))
            .ReturnsAsync([MakeRole(id: 10)]);
        var state = MakeState(assignments:
        [
            new InviteApplicationAssignment
            {
                OrganisationApplicationId = 5,
                ApplicationRoleId = 10,
                ApplicationRoleIds = null
            }
        ]);

        var result = await _sut.GetApplicationRolesStepAsync("test-org", state, CancellationToken.None);

        result!.Applications[0].GiveAccess.Should().BeFalse();
        result.Applications[0].SelectedRoleId.Should().BeNull();
    }

    // ── GetCheckAnswersViewModelAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetCheckAnswersViewModelAsync_OrgNotFound_ReturnsNull()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.GetCheckAnswersViewModelAsync(
            "slug", MakeState(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCheckAnswersViewModelAsync_MapsAssignmentsToRoleNames()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([MakeApplication(orgAppId: 5, name: "App One")]);
        _adapter.Setup(a => a.GetApplicationRolesAsync(OrgId, It.IsAny<int>(), default))
            .ReturnsAsync([MakeRole(id: 10, name: "Admin")]);
        var state = MakeState(assignments:
        [
            new InviteApplicationAssignment
            {
                OrganisationApplicationId = 5,
                ApplicationRoleId = 10,
                ApplicationRoleIds = null
            }
        ]);

        var result = await _sut.GetCheckAnswersViewModelAsync("test-org", state, CancellationToken.None);

        result!.Applications.Should().HaveCount(1);
        result.Applications[0].ApplicationName.Should().Be("App One");
        result.Applications[0].RoleName.Should().Be("Admin");
    }

    [Fact]
    public async Task GetCheckAnswersViewModelAsync_AssignmentWithUnknownApp_IsExcluded()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([]);
        var state = MakeState(assignments:
        [
            new InviteApplicationAssignment
            {
                OrganisationApplicationId = 999,
                ApplicationRoleId = 10,
                ApplicationRoleIds = null
            }
        ]);

        var result = await _sut.GetCheckAnswersViewModelAsync("test-org", state, CancellationToken.None);

        result!.Applications.Should().BeEmpty();
    }

    // ── InviteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task InviteAsync_OrgNotFound_ReturnsNotFound()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.InviteAsync("slug", MakeState(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Match(_ => false, o => o == ServiceOutcome.NotFound).Should().BeTrue();
    }

    [Fact]
    public async Task InviteAsync_BuildsCorrectRequestFromState()
    {
        SetupOrg();
        InviteUserRequest? captured = null;
        _adapter.Setup(a => a.InviteUserAsync(OrgGuid, It.IsAny<InviteUserRequest>(), default))
            .Callback<Guid, InviteUserRequest, CancellationToken>((_, r, _) => captured = r)
            .ReturnsAsync(SuccessResult());

        var state = MakeState(
            email: "jane@example.com",
            firstName: "Jane",
            lastName: "Doe",
            role: OrganisationRole.Admin,
            assignments:
            [
                new InviteApplicationAssignment
                {
                    OrganisationApplicationId = 5,
                    ApplicationRoleId = 10,
                    ApplicationRoleIds = null
                }
            ]);

        await _sut.InviteAsync("test-org", state, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Email.Should().Be("jane@example.com");
        captured.FirstName.Should().Be("Jane");
        captured.LastName.Should().Be("Doe");
        captured.OrganisationRole.Should().Be(OrganisationRole.Admin);
        captured.ApplicationAssignments.Should().HaveCount(1);
        captured.ApplicationAssignments![0].OrganisationApplicationId.Should().Be(5);
        captured.ApplicationAssignments[0].ApplicationRoleIds.Should().Contain(10);
    }

    [Fact]
    public async Task InviteAsync_PassesThroughAdapterResult()
    {
        SetupOrg();
        _adapter.Setup(a => a.InviteUserAsync(OrgGuid, It.IsAny<InviteUserRequest>(), default))
            .ReturnsAsync(SuccessResult());

        var result = await _sut.InviteAsync("test-org", MakeState(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Match(_ => false, o => o == ServiceOutcome.Success).Should().BeTrue();
    }

    // ── ResendInviteAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ResendInviteAsync_OrgNotFound_ReturnsNotFound()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.ResendInviteAsync("slug", Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Match(_ => false, o => o == ServiceOutcome.NotFound).Should().BeTrue();
    }

    [Fact]
    public async Task ResendInviteAsync_PassesCorrectIdsToAdapter()
    {
        var inviteGuid = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.ResendInviteAsync(OrgGuid, inviteGuid, default))
            .ReturnsAsync(SuccessResult());

        var result = await _sut.ResendInviteAsync("test-org", inviteGuid, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _adapter.Verify(a => a.ResendInviteAsync(OrgGuid, inviteGuid, default), Times.Once);
    }
}