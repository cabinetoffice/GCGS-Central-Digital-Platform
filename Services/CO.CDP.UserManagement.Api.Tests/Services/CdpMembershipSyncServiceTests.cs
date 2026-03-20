using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class CdpMembershipSyncServiceTests
{
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository;
    private readonly Mock<IRoleMappingService> _roleMappingService;
    private readonly Mock<IOrganisationPersonSyncRepository> _syncRepository;
    private readonly Mock<IFeatureManager> _featureManager;
    private readonly CdpMembershipSyncService _service;

    private static readonly Guid OrgGuid = Guid.NewGuid();
    private static readonly Guid PersonGuid = Guid.NewGuid();

    private static readonly UmOrganisation Org = new()
    {
        Id = 10,
        CdpOrganisationGuid = OrgGuid,
        Name = "Org",
        Slug = "org"
    };

    public CdpMembershipSyncServiceTests()
    {
        _membershipRepository = new Mock<IUserOrganisationMembershipRepository>();
        _roleMappingService = new Mock<IRoleMappingService>();
        _syncRepository = new Mock<IOrganisationPersonSyncRepository>();
        _featureManager = new Mock<IFeatureManager>();
        _featureManager.Setup(f => f.IsEnabledAsync(Infrastructure.Constants.FeatureFlags.OrganisationSyncEnabled))
            .ReturnsAsync(true);
        _service = new CdpMembershipSyncService(
            _membershipRepository.Object,
            _roleMappingService.Object,
            _syncRepository.Object,
            _featureManager.Object,
            Mock.Of<ILogger<CdpMembershipSyncService>>());
    }

    [Fact]
    public async Task SyncMembershipCreatedAsync_WhenEnabled_UpsertsOrganisationPerson()
    {
        var membership = ActiveMembership(OrganisationRole.Admin);
        SetupMembershipRepoReturns(membership);
        _roleMappingService.Setup(r => r.ShouldSyncToOrganisationInformationAsync(membership.Id, default)).ReturnsAsync(true);
        _roleMappingService.Setup(r => r.GetOrganisationInformationScopesAsync(membership.Id, default))
            .ReturnsAsync(["ADMIN", "RESPONDER"]);

        await _service.SyncMembershipCreatedAsync(membership);

        _syncRepository.Verify(r => r.UpsertAsync(
            OrgGuid, PersonGuid,
            It.Is<IReadOnlyList<string>>(s => s.Contains("ADMIN") && s.Contains("RESPONDER")),
            default), Times.Once);
    }

    [Fact]
    public async Task SyncMembershipRoleChangedAsync_WhenEnabled_UpsertsOrganisationPerson()
    {
        var membership = ActiveMembership(OrganisationRole.Admin);
        SetupMembershipRepoReturns(membership);
        _roleMappingService.Setup(r => r.ShouldSyncToOrganisationInformationAsync(membership.Id, default)).ReturnsAsync(true);
        _roleMappingService.Setup(r => r.GetOrganisationInformationScopesAsync(membership.Id, default))
            .ReturnsAsync(["ADMIN", "RESPONDER"]);

        await _service.SyncMembershipRoleChangedAsync(membership);

        _syncRepository.Verify(r => r.UpsertAsync(
            OrgGuid, PersonGuid,
            It.IsAny<IReadOnlyList<string>>(),
            default), Times.Once);
    }

    [Fact]
    public async Task SyncMembershipRemovedAsync_RemovesOrganisationPerson()
    {
        var membership = ActiveMembership(OrganisationRole.Member);

        await _service.SyncMembershipRemovedAsync(membership);

        _syncRepository.Verify(r => r.RemoveAsync(OrgGuid, PersonGuid, default), Times.Once);
    }

    [Fact]
    public async Task SyncMembershipCreatedAsync_WithoutCdpPersonId_DoesNotSync()
    {
        var membership = ActiveMembership(OrganisationRole.Admin, hasCdpPersonId: false);
        SetupMembershipRepoReturns(membership);
        _roleMappingService.Setup(r => r.ShouldSyncToOrganisationInformationAsync(membership.Id, default)).ReturnsAsync(true);

        await _service.SyncMembershipCreatedAsync(membership);

        _syncRepository.Verify(r => r.UpsertAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncMembershipAccessChangedAsync_WhenRoleNotSyncEnabled_DoesNotSync()
    {
        var membership = ActiveMembership(OrganisationRole.Agent);
        SetupMembershipRepoReturns(membership);
        _roleMappingService.Setup(r => r.ShouldSyncToOrganisationInformationAsync(membership.Id, default)).ReturnsAsync(false);

        await _service.SyncMembershipAccessChangedAsync(membership.Id);

        _syncRepository.Verify(r => r.UpsertAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncMembershipCreatedAsync_WhenOrganisationSyncDisabled_DoesNotSync()
    {
        _featureManager.Setup(f => f.IsEnabledAsync(Infrastructure.Constants.FeatureFlags.OrganisationSyncEnabled))
            .ReturnsAsync(false);
        var membership = ActiveMembership(OrganisationRole.Admin);

        await _service.SyncMembershipCreatedAsync(membership);

        _syncRepository.Verify(r => r.UpsertAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncMembershipRemovedAsync_WhenOrganisationSyncDisabled_DoesNotRemove()
    {
        _featureManager.Setup(f => f.IsEnabledAsync(Infrastructure.Constants.FeatureFlags.OrganisationSyncEnabled))
            .ReturnsAsync(false);
        var membership = ActiveMembership(OrganisationRole.Member);

        await _service.SyncMembershipRemovedAsync(membership);

        _syncRepository.Verify(r => r.RemoveAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static UserOrganisationMembership ActiveMembership(OrganisationRole role, bool hasCdpPersonId = true) =>
        new()
        {
            Id = 1,
            OrganisationId = Org.Id,
            Organisation = Org,
            OrganisationRoleId = (int)role,
            IsActive = true,
            CdpPersonId = hasCdpPersonId ? PersonGuid : null,
            UserPrincipalId = "user-urn"
        };

    private void SetupMembershipRepoReturns(UserOrganisationMembership membership) =>
        _membershipRepository
            .Setup(r => r.GetWithOrganisationAndRoleAsync(membership.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);
}
