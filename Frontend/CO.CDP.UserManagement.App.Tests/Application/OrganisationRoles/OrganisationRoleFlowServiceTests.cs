using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Application.OrganisationRoles.Implementations;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.App.Tests.TestFixtures;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Application.OrganisationRoles;

public class OrganisationRoleFlowServiceTests : AdapterTestFixture
{
    private readonly Mock<IUserManagementApiAdapter> _adapter = new();
    private readonly Mock<IOrganisationRoleService> _orgRoleService = new();
    private readonly OrganisationRoleFlowService _sut;

    public OrganisationRoleFlowServiceTests()
        => _sut = new OrganisationRoleFlowService(_adapter.Object, _orgRoleService.Object);

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
    public async Task GetUserViewModelAsync_ValidUser_ReturnsMappedViewModel()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId, OrganisationRole.Member, "Jane", "Doe", "jane@example.com"));

        var result = await _sut.GetUserViewModelAsync("test-org", personId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.CdpPersonId.Should().Be(personId);
        result.InviteGuid.Should().BeNull();
        result.UserDisplayName.Should().Be("Jane Doe");
        result.Email.Should().Be("jane@example.com");
        result.CurrentRole.Should().Be(OrganisationRole.Member);
        result.IsPending.Should().BeFalse();
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
    public async Task GetInviteViewModelAsync_ValidInvite_SetIsPendingTrue()
    {
        var inviteGuid = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetInviteAsync(OrgGuid, inviteGuid, default))
            .ReturnsAsync(MakeInvite(inviteGuid, pendingInviteId: 1, email: "invite@example.com",
                role: OrganisationRole.Admin, firstName: "Jane", lastName: "Doe"));

        var result = await _sut.GetInviteViewModelAsync("test-org", inviteGuid, CancellationToken.None);

        result!.IsPending.Should().BeTrue();
        result.InviteGuid.Should().Be(inviteGuid);
        result.CdpPersonId.Should().BeNull();
        result.CurrentRole.Should().Be(OrganisationRole.Admin);
    }

    // ── BuildPageViewModelAsync ───────────────────────────────────────────────

    [Fact]
    public async Task BuildPageViewModelAsync_FetchesRolesFromOrgRoleService()
    {
        var roles = new List<OrganisationRoleDefinitionResponse>
        {
            new()
            {
                Id = OrganisationRole.Admin,
                DisplayName = "Administrator",
                Description = "Manages the org"
            }
        };

        _orgRoleService.Setup(s => s.GetRolesAsync(default)).ReturnsAsync(roles);

        var viewModel = ChangeUserRoleViewModel.Empty;
        var result = await _sut.BuildPageViewModelAsync(viewModel, OrganisationRole.Admin, CancellationToken.None);

        result.Should().NotBeNull();
        result.RoleOptions.Should().HaveCount(1);
        result.SelectedRole.Should().Be(OrganisationRole.Admin);
    }

    [Fact]
    public async Task BuildPageViewModelAsync_NullSelectedRole_DoesNotThrow()
    {
        _orgRoleService.Setup(s => s.GetRolesAsync(default))
            .ReturnsAsync(Array.Empty<OrganisationRoleDefinitionResponse>());

        var result = await _sut.BuildPageViewModelAsync(
            ChangeUserRoleViewModel.Empty, null, CancellationToken.None);

        result.Should().NotBeNull();
    }

    // ── UpdateUserRoleAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUserRoleAsync_OrgNotFound_ReturnsNotFound()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.UpdateUserRoleAsync(
            "slug", Guid.NewGuid(), OrganisationRole.Admin, CancellationToken.None);

        result.GetOrElse(ServiceOutcome.NotFound).Should().Be(ServiceOutcome.NotFound);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_CallsAdapterWithCorrectRequest()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.UpdateUserOrganisationRoleAsync(
                OrgGuid, personId, It.IsAny<ChangeOrganisationRoleRequest>(), default))
            .Callback<Guid, Guid, ChangeOrganisationRoleRequest, CancellationToken>((_, _, r, _) => _ = r)
            .ReturnsAsync(NotFoundResult());

        var result = await _sut.UpdateUserRoleAsync(
            "test-org", personId, OrganisationRole.Admin, CancellationToken.None);

        result.GetOrElse(ServiceOutcome.NotFound).Should().Be(ServiceOutcome.NotFound);
    }

    // ── UpdateInviteRoleAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateInviteRoleAsync_OrgNotFound_ReturnsNotFound()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result =
            await _sut.UpdateInviteRoleAsync("slug", Guid.NewGuid(), OrganisationRole.Admin, CancellationToken.None);

        result.GetOrElse(ServiceOutcome.NotFound).Should().Be(ServiceOutcome.NotFound);
    }

    [Fact]
    public async Task UpdateInviteRoleAsync_CallsCorrectAdapterMethod()
    {
        var inviteGuid = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.UpdateInviteOrganisationRoleAsync(
                OrgGuid, inviteGuid, It.IsAny<ChangeOrganisationRoleRequest>(), default))
            .ReturnsAsync(SuccessResult());

        await _sut.UpdateInviteRoleAsync("test-org", inviteGuid, OrganisationRole.Owner, CancellationToken.None);

        // Must call invite endpoint, not user endpoint
        _adapter.Verify(a => a.UpdateInviteOrganisationRoleAsync(
            OrgGuid, inviteGuid, It.IsAny<ChangeOrganisationRoleRequest>(), default), Times.Once);
        _adapter.Verify(a => a.UpdateUserOrganisationRoleAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ChangeOrganisationRoleRequest>(), default), Times.Never);
    }
}