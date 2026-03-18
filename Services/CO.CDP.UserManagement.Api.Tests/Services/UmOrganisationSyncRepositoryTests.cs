using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Repositories;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Moq;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class UmOrganisationSyncRepositoryTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepository = new();
    private readonly Mock<ISlugGeneratorService> _slugGeneratorService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private UmOrganisationSyncRepository CreateSut() => new(
        _organisationRepository.Object,
        _membershipRepository.Object,
        _slugGeneratorService.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_AddsOwnerMembership_WhenFounderDoesNotExist()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);
        _membershipRepository
            .Setup(r => r.GetByUserAndOrganisationAsync("urn:example:user", organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);

        await CreateSut().EnsureFounderOwnerCreatedAsync(organisationGuid, personGuid, "urn:example:user");

        _membershipRepository.Verify(r => r.Add(It.Is<UserOrganisationMembership>(m =>
            m.UserPrincipalId == "urn:example:user" &&
            m.CdpPersonId == personGuid &&
            m.OrganisationId == organisation.Id &&
            m.OrganisationRoleId == (int)OrganisationRole.Owner &&
            m.IsActive &&
            m.CreatedBy == "system:org-sync")), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_DoesNothing_WhenFounderMembershipAlreadyExistsByPerson()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership { Id = 7, OrganisationId = organisation.Id, UserPrincipalId = "urn:example:user" });

        await CreateSut().EnsureFounderOwnerCreatedAsync(organisationGuid, personGuid, "urn:example:user");

        _membershipRepository.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_DoesNothing_WhenFounderMembershipAlreadyExistsByUserPrincipal()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var organisation = new UmOrganisation
        {
            Id = 42,
            CdpOrganisationGuid = organisationGuid,
            Name = "Acme",
            Slug = "acme",
            IsActive = true,
            CreatedBy = "seed"
        };

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisation);
        _membershipRepository
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(personGuid, organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);
        _membershipRepository
            .Setup(r => r.GetByUserAndOrganisationAsync("urn:example:user", organisation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership { Id = 7, OrganisationId = organisation.Id, UserPrincipalId = "urn:example:user" });

        await CreateSut().EnsureFounderOwnerCreatedAsync(organisationGuid, personGuid, "urn:example:user");

        _membershipRepository.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsureFounderOwnerCreatedAsync_Throws_WhenOrganisationHasNotBeenSynced()
    {
        var organisationGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();

        _organisationRepository
            .Setup(r => r.GetByCdpGuidAsync(organisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UmOrganisation?)null);

        var act = () => CreateSut().EnsureFounderOwnerCreatedAsync(organisationGuid, personGuid, "urn:example:user");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{organisationGuid}*");
    }
}
