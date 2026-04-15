using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Application.Removal.Implementations;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Tests.TestFixtures;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Removal;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Application.Removal;

public class UserRemovalServiceTests : AdapterTestFixture
{
    private readonly Mock<IUserManagementApiAdapter> _adapter = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly UserRemovalService _sut;

    public UserRemovalServiceTests()
        => _sut = new UserRemovalService(_adapter.Object, _currentUserService.Object);

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

        var result = await _sut.GetInviteViewModelAsync("slug", 42, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInviteViewModelAsync_InviteNotFound_ReturnsNull()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default)).ReturnsAsync(Array.Empty<PendingOrganisationInviteResponse>());

        var result = await _sut.GetInviteViewModelAsync("test-org", 999, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInviteViewModelAsync_ValidInvite_SetsIsPendingTrue()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetInvitesAsync(OrgGuid, default))
            .ReturnsAsync(new[] { MakeInvite(pendingInviteId: 42) });

        var result = await _sut.GetInviteViewModelAsync("test-org", 42, CancellationToken.None);

        result!.PendingInviteId.Should().Be(42);
    }

    // ── ValidateRemovalAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ValidateRemovalAsync_OrgNotFound_ReturnsFail()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.ValidateRemovalAsync("slug", Guid.NewGuid(), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateRemovalAsync_UserNotFound_ReturnsFail()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, It.IsAny<Guid>(), default))
            .ReturnsAsync((OrganisationUserResponse?)null);

        var result = await _sut.ValidateRemovalAsync("test-org", Guid.NewGuid(), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateRemovalAsync_SelfRemoval_ReturnsFail()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId, OrganisationRole.Admin, email: "me@example.com"));
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync(new[] { MakeUser(personId, OrganisationRole.Admin), MakeUser(Guid.NewGuid(), OrganisationRole.Owner) });
        _currentUserService.Setup(s => s.GetUserEmail()).Returns("me@example.com");
        _currentUserService.Setup(s => s.GetOrganisationRole(OrgGuid)).Returns(OrganisationRole.Admin);

        var result = await _sut.ValidateRemovalAsync("test-org", personId, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot remove yourself");
    }

    [Fact]
    public async Task ValidateRemovalAsync_AdminRemovingOwner_ReturnsFail()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId, OrganisationRole.Owner, email: "owner@example.com"));
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync(new[] { MakeUser(personId, OrganisationRole.Owner), MakeUser(Guid.NewGuid(), OrganisationRole.Owner) });
        _currentUserService.Setup(s => s.GetUserEmail()).Returns("admin@example.com");
        _currentUserService.Setup(s => s.GetOrganisationRole(OrgGuid)).Returns(OrganisationRole.Admin);

        var result = await _sut.ValidateRemovalAsync("test-org", personId, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("permission");
    }

    [Fact]
    public async Task ValidateRemovalAsync_LastOwner_ReturnsFail()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId, OrganisationRole.Owner, email: "owner@example.com"));
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync(new[] { MakeUser(personId, OrganisationRole.Owner) });
        _currentUserService.Setup(s => s.GetUserEmail()).Returns("other@example.com");
        _currentUserService.Setup(s => s.GetOrganisationRole(OrgGuid)).Returns(OrganisationRole.Owner);

        var result = await _sut.ValidateRemovalAsync("test-org", personId, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("last owner");
    }

    [Fact]
    public async Task ValidateRemovalAsync_ValidRemoval_ReturnsSuccess()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId, OrganisationRole.Member, email: "member@example.com"));
        _adapter.Setup(a => a.GetUsersAsync(OrgGuid, default))
            .ReturnsAsync(new[] { MakeUser(Guid.NewGuid(), OrganisationRole.Owner), MakeUser(personId, OrganisationRole.Member) });
        _currentUserService.Setup(s => s.GetUserEmail()).Returns("admin@example.com");
        _currentUserService.Setup(s => s.GetOrganisationRole(OrgGuid)).Returns(OrganisationRole.Owner);

        var result = await _sut.ValidateRemovalAsync("test-org", personId, CancellationToken.None);

        result.IsValid.Should().BeTrue();
    }

    // ── RemoveInviteAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveInviteAsync_OrgNotFound_ReturnsNotFound()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.RemoveInviteAsync("slug", 42, CancellationToken.None);

        result.Should().BeOfType<InviteRemovalSubmitResult.NotFound>();
    }

    [Fact]
    public async Task RemoveInviteAsync_CallsAdapterWithCorrectIds()
    {
        SetupOrg();
        _adapter.Setup(a => a.CancelInviteAsync(OrgGuid, 42, default))
            .ReturnsAsync(SuccessResult());

        var result = await _sut.RemoveInviteAsync("test-org", 42, CancellationToken.None);

        result.Should().BeOfType<InviteRemovalSubmitResult.Removed>();
        _adapter.Verify(a => a.CancelInviteAsync(OrgGuid, 42, default), Times.Once);
    }

    [Fact]
    public async Task RemoveInviteAsync_AdapterReturnsFailure_ReturnsNotFound()
    {
        SetupOrg();
        _adapter.Setup(a => a.CancelInviteAsync(OrgGuid, 42, default))
            .ReturnsAsync(NotFoundResult());

        var result = await _sut.RemoveInviteAsync("test-org", 42, CancellationToken.None);

        result.Should().BeOfType<InviteRemovalSubmitResult.NotFound>();
    }
}