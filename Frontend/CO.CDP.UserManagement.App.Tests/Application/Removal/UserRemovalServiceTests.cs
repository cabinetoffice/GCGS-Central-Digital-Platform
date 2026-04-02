using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Application.Removal.Implementations;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.App.Tests.TestFixtures;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Application.Removal;

public class UserRemovalServiceTests : AdapterTestFixture
{
    private readonly Mock<IUserManagementApiAdapter> _adapter = new();
    private readonly UserRemovalService _sut;

    public UserRemovalServiceTests()
        => _sut = new UserRemovalService(_adapter.Object);

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
    public async Task GetUserViewModelAsync_ValidUser_MapsViewModel()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId, OrganisationRole.Member, "Jane", "Doe", "jane@example.com"));

        var result = await _sut.GetUserViewModelAsync("test-org", personId, CancellationToken.None);

        result!.UserDisplayName.Should().Be("Jane Doe");
        result.Email.Should().Be("jane@example.com");
        result.CurrentRole.Should().Be(OrganisationRole.Member);
        result.PendingInviteId.Should().BeNull();
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
        var inviteGuid = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetInviteAsync(OrgGuid, inviteGuid, default))
            .ReturnsAsync((PendingOrganisationInviteResponse?)null);

        var result = await _sut.GetInviteViewModelAsync("test-org", inviteGuid, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInviteViewModelAsync_ValidInvite_SetsIsPendingTrue()
    {
        var inviteGuid = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetInviteAsync(OrgGuid, inviteGuid, default))
            .ReturnsAsync(MakeInvite(inviteGuid: inviteGuid, pendingInviteId: 42));

        var result = await _sut.GetInviteViewModelAsync("test-org", inviteGuid, CancellationToken.None);

        result!.PendingInviteId.Should().Be(42);
    }

    // ── IsLastOwnerAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task IsLastOwnerAsync_OrgNotFound_ReturnsFalse()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.IsLastOwnerAsync("slug", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsLastOwnerAsync_MultipleOwners_ReturnsFalse()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync(new[] {
                MakeUser(personId, OrganisationRole.Owner),
                MakeUser(Guid.NewGuid(), OrganisationRole.Owner)
            });

        var result = await _sut.IsLastOwnerAsync("test-org", personId, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsLastOwnerAsync_OneOwnerButNotThisUser_ReturnsFalse()
    {
        var personId   = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync(new[] { MakeUser(differentId, OrganisationRole.Owner) });

        var result = await _sut.IsLastOwnerAsync("test-org", personId, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsLastOwnerAsync_ExactlyOneOwnerAndIsThisUser_ReturnsTrue()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync(new[] {
                MakeUser(personId, OrganisationRole.Owner),
                MakeUser(Guid.NewGuid(), OrganisationRole.Admin)
            });

        var result = await _sut.IsLastOwnerAsync("test-org", personId, CancellationToken.None);

        result.Should().BeTrue();
    }

    // ── RemoveUserAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveUserAsync_OrgNotFound_ReturnsNotFound()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.RemoveUserAsync("slug", Guid.NewGuid(), CancellationToken.None);

        result.GetOrElse(ServiceOutcome.NotFound).Should().Be(ServiceOutcome.NotFound);
    }

    [Fact]
    public async Task RemoveUserAsync_CallsAdapterWithCorrectIds()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.RemoveUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(SuccessResult());

        var result = await _sut.RemoveUserAsync("test-org", personId, CancellationToken.None);

        result.GetOrElse(ServiceOutcome.NotFound).Should().Be(ServiceOutcome.Success);
        _adapter.Verify(a => a.RemoveUserAsync(OrgGuid, personId, default), Times.Once);
    }

    // ── RemoveInviteAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveInviteAsync_OrgNotFound_ReturnsNotFound()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.RemoveInviteAsync("slug", Guid.NewGuid(), CancellationToken.None);

        result.GetOrElse(ServiceOutcome.NotFound).Should().Be(ServiceOutcome.NotFound);
    }

    [Fact]
    public async Task RemoveInviteAsync_InviteNotFound_ReturnsNotFound()
    {
        var inviteGuid = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetInviteAsync(OrgGuid, inviteGuid, default))
            .ReturnsAsync((PendingOrganisationInviteResponse?)null);

        var result = await _sut.RemoveInviteAsync("test-org", inviteGuid, CancellationToken.None);

        result.GetOrElse(ServiceOutcome.NotFound).Should().Be(ServiceOutcome.NotFound);
    }

    [Fact]
    public async Task RemoveInviteAsync_CallsAdapterWithCorrectIds()
    {
        var inviteGuid = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetInviteAsync(OrgGuid, inviteGuid, default))
            .ReturnsAsync(MakeInvite(inviteGuid: inviteGuid, pendingInviteId: 42));
        _adapter.Setup(a => a.CancelInviteAsync(OrgGuid, 42, default))
            .ReturnsAsync(SuccessResult());

        var result = await _sut.RemoveInviteAsync("test-org", inviteGuid, CancellationToken.None);

        result.GetOrElse(ServiceOutcome.NotFound).Should().Be(ServiceOutcome.Success);
        _adapter.Verify(a => a.CancelInviteAsync(OrgGuid, 42, default), Times.Once);
    }
}