using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Application.ApplicationRoles.Implementations;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.App.Tests.TestFixtures;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Application.ApplicationRoles;

public class ApplicationRoleFlowServiceTests : AdapterTestFixture
{
    private readonly Mock<IUserManagementApiAdapter> _adapter = new();
    private readonly ApplicationRoleFlowService _sut;

    public ApplicationRoleFlowServiceTests()
        => _sut = new ApplicationRoleFlowService(_adapter.Object);

    private void SetupOrg(string name = "Test Org") => base.SetupOrg(_adapter, name);

    // ── GetUserViewModelAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetUserViewModelAsync_OrgNotFound_ReturnsNull()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.GetUserViewModelAsync("slug", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserViewModelAsync_UserNotFound_ReturnsNull()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, It.IsAny<Guid>(), default))
            .ReturnsAsync((OrganisationUserResponse?)null);

        var result = await _sut.GetUserViewModelAsync("test-org", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserViewModelAsync_SetsIsPendingFalse_AndCdpPersonId()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId: personId));
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([]);

        var result = await _sut.GetUserViewModelAsync("test-org", personId, CancellationToken.None);

        result!.CdpPersonId.Should().Be(personId);
        result.InviteGuid.Should().BeNull();
    }

    [Fact]
    public async Task GetUserViewModelAsync_FetchesRolesForAllApplicationsInParallel()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId: personId));
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([MakeApplication(appId: 1), MakeApplication(appId: 2)]);
        _adapter.Setup(a => a.GetApplicationRolesAsync(OrgId, 1, default))
            .ReturnsAsync([MakeRole(10, "Admin")]);
        _adapter.Setup(a => a.GetApplicationRolesAsync(OrgId, 2, default))
            .ReturnsAsync([MakeRole(20, "Viewer")]);

        var result = await _sut.GetUserViewModelAsync("test-org", personId, CancellationToken.None);

        result!.Applications.Should().HaveCount(2);
        _adapter.Verify(a => a.GetApplicationRolesAsync(OrgId, 1, default), Times.Once);
        _adapter.Verify(a => a.GetApplicationRolesAsync(OrgId, 2, default), Times.Once);
    }

    [Fact]
    public async Task GetUserViewModelAsync_UsesOrganisationFilteredRolesAndShowsFewerRolesWhenFiltered()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId: personId));
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([MakeApplication(appId: 1)]);
        // Simulate backend filtering: only one role for this org
        _adapter.Setup(a => a.GetApplicationRolesAsync(OrgId, 1, default))
            .ReturnsAsync([MakeRole(10, "Admin")]);

        var result = await _sut.GetUserViewModelAsync("test-org", personId, CancellationToken.None);

        result!.Applications.Should().HaveCount(1);
        result.Applications[0].Roles.Should().HaveCount(1);
        _adapter.Verify(a => a.GetApplicationRolesAsync(OrgId, 1, default), Times.Once);
    }

    [Fact]
    public async Task GetUserViewModelAsync_SetsHasExistingAccess_WhenUserHasRole()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(
                personId: personId,
                applicationRoles:
                [
                    new UserAssignmentResponse
                    {
                        OrganisationApplicationId = 5,
                        Id = 10,
                        UserOrganisationMembershipId = 0,
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    }
                ]));
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([MakeApplication(orgAppId: 5, appId: 1)]);
        _adapter.Setup(a => a.GetApplicationRolesAsync(OrgId, 1, default))
            .ReturnsAsync([MakeRole(10, "Admin")]);

        var result = await _sut.GetUserViewModelAsync("test-org", personId, CancellationToken.None);

        result!.Applications[0].HasExistingAccess.Should().BeTrue();
        result.Applications[0].GiveAccess.Should().BeTrue();
        result.Applications[0].SelectedRoleId.Should().Be(10);
    }

    [Fact]
    public async Task GetUserViewModelAsync_SetsHasExistingAccess_FalseWhenUserHasNoRole()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId: personId));
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([MakeApplication(orgAppId: 5, appId: 1)]);
        _adapter.Setup(a => a.GetApplicationRolesAsync(OrgId, 1, default))
            .ReturnsAsync([MakeRole(10)]);

        var result = await _sut.GetUserViewModelAsync("test-org", personId, CancellationToken.None);

        result!.Applications[0].HasExistingAccess.Should().BeFalse();
        result.Applications[0].GiveAccess.Should().BeFalse();
        result.Applications[0].SelectedRoleId.Should().BeNull();
    }

    // ── GetInviteViewModelAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetInviteViewModelAsync_OrgNotFound_ReturnsNull()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.GetInviteViewModelAsync("slug", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInviteViewModelAsync_InviteNotFound_ReturnsNull()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetInviteAsync(OrgGuid, It.IsAny<Guid>(), default))
            .ReturnsAsync((PendingOrganisationInviteResponse?)null);

        var result = await _sut.GetInviteViewModelAsync("test-org", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInviteViewModelAsync_SetsInviteGuid_AndNullCdpPersonId()
    {
        var inviteGuid = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetInviteAsync(OrgGuid, inviteGuid, default))
            .ReturnsAsync(MakeInvite(inviteGuid: inviteGuid));
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync([]);

        var result = await _sut.GetInviteViewModelAsync("test-org", inviteGuid, CancellationToken.None);

        result!.InviteGuid.Should().Be(inviteGuid);
        result.CdpPersonId.Should().BeNull();
    }

    // ── UpdateUserRolesAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUserRolesAsync_OrgNotFound_ReturnsNotFound()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.UpdateUserRolesAsync("slug", Guid.NewGuid(), [], CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Match(_ => false, o => o == ServiceOutcome.NotFound).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserRolesAsync_BuildsCorrectRequestFromAssignments()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        UpdateUserAssignmentsRequest? captured = null;
        _adapter.Setup(a => a.UpdateUserApplicationRolesAsync(
                OrgId, personId, It.IsAny<UpdateUserAssignmentsRequest>(), default))
            .Callback<int, Guid, UpdateUserAssignmentsRequest, CancellationToken>((_, _, r, _) => captured = r)
            .ReturnsAsync(SuccessResult());

        var assignments = new List<ApplicationRoleAssignmentPostModel>
        {
            new()
            {
                OrganisationApplicationId = 5,
                ApplicationId = 1,
                GiveAccess = true,
                SelectedRoleId = 10,
                SelectedRoleIds = null
            }
        };

        await _sut.UpdateUserRolesAsync("test-org", personId, assignments, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Assignments.Should().HaveCount(1);
        var assignment = captured.Assignments.ElementAt(0);
        assignment.OrganisationApplicationId.Should().Be(5);
        assignment.RoleIds.Should().ContainSingle().Which.Should().Be(10);
    }

    [Fact]
    public async Task UpdateUserRolesAsync_CallsUserEndpoint_NotInviteEndpoint()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.UpdateUserApplicationRolesAsync(
                OrgId, personId, It.IsAny<UpdateUserAssignmentsRequest>(), default))
            .ReturnsAsync(SuccessResult());

        await _sut.UpdateUserRolesAsync("test-org", personId, [], CancellationToken.None);

        _adapter.Verify(a => a.UpdateUserApplicationRolesAsync(
            OrgId, personId, It.IsAny<UpdateUserAssignmentsRequest>(), default), Times.Once);
        _adapter.Verify(a => a.UpdateInviteApplicationRolesAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<UpdateUserAssignmentsRequest>(), default), Times.Never);
    }

    // ── UpdateInviteRolesAsync ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateInviteRolesAsync_CallsInviteEndpoint_NotUserEndpoint()
    {
        var inviteGuid = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.UpdateInviteApplicationRolesAsync(
                OrgGuid, inviteGuid, It.IsAny<UpdateUserAssignmentsRequest>(), default))
            .ReturnsAsync(SuccessResult());

        await _sut.UpdateInviteRolesAsync("test-org", inviteGuid, [], CancellationToken.None);

        _adapter.Verify(a => a.UpdateInviteApplicationRolesAsync(
            OrgGuid, inviteGuid, It.IsAny<UpdateUserAssignmentsRequest>(), default), Times.Once);
        _adapter.Verify(a => a.UpdateUserApplicationRolesAsync(
            It.IsAny<int>(), It.IsAny<Guid>(),
            It.IsAny<UpdateUserAssignmentsRequest>(), default), Times.Never);
    }
}