using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using CdpOrganisation = CO.CDP.OrganisationInformation.Persistence.Organisation;
using CdpPerson = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class OrganisationPersonsSyncServiceTests
{
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepositoryMock;
    private readonly Mock<IUserAssignmentService> _userAssignmentServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<OrganisationPersonsSyncService>> _loggerMock;

    public OrganisationPersonsSyncServiceTests()
    {
        _membershipRepositoryMock = new Mock<IUserOrganisationMembershipRepository>();
        _userAssignmentServiceMock = new Mock<IUserAssignmentService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<OrganisationPersonsSyncService>>();

        _membershipRepositoryMock
            .Setup(r => r.GetByOrganisationIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _membershipRepositoryMock
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOrganisationMembership?)null);

        _userAssignmentServiceMock
            .Setup(s => s.AssignDefaultApplicationsAsync(It.IsAny<UserOrganisationMembership>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task SyncOrganisationMembershipsAsync_WhenNoPersonsFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var orgGuid = Guid.NewGuid();
        var context = CreateContext();
        SeedOrganisationWithoutPersons(context, orgGuid);
        var service = BuildService(context);

        // Act
        var act = async () => await service.SyncOrganisationMembershipsAsync(orgGuid, 1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"No persons found for organisation {orgGuid}*");
    }

    [Fact]
    public async Task SyncOrganisationMembershipsAsync_WhenEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var context = CreateContext();
        var service = BuildService(context);

        // Act
        var act = async () => await service.SyncOrganisationMembershipsAsync(Guid.Empty, 1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Organisation CDP GUID cannot be empty*");
    }

    [Fact]
    public async Task SyncOrganisationMembershipsAsync_WhenInvalidOrgId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var context = CreateContext();
        var service = BuildService(context);

        // Act
        var act = async () => await service.SyncOrganisationMembershipsAsync(Guid.NewGuid(), 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*Organisation ID must be positive*");
    }

    [Fact]
    public async Task SyncOrganisationMembershipsAsync_WhenFirstAdminExists_AssignsOwnerRole()
    {
        // Arrange
        var orgGuid = Guid.NewGuid();
        const int orgId = 42;
        var context = CreateContext();
        SeedOrganisationWithAdminPerson(context, orgGuid);

        UserOrganisationMembership? capturedMembership = null;
        _membershipRepositoryMock
            .Setup(r => r.Add(It.IsAny<UserOrganisationMembership>()))
            .Callback<UserOrganisationMembership>(m => capturedMembership = m);

        var service = BuildService(context);

        // Act
        await service.SyncOrganisationMembershipsAsync(orgGuid, orgId);

        // Assert — first admin with ADMIN scope → Owner
        capturedMembership.Should().NotBeNull();
        capturedMembership!.OrganisationRole.Should().Be(OrganisationRole.Owner);
        capturedMembership.OrganisationId.Should().Be(orgId);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _userAssignmentServiceMock.Verify(
            s => s.AssignDefaultApplicationsAsync(It.IsAny<UserOrganisationMembership>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SyncOrganisationMembershipsAsync_WhenPersonHasNoAdminScope_AssignsMemberRole()
    {
        // Arrange
        var orgGuid = Guid.NewGuid();
        const int orgId = 42;
        var context = CreateContext();
        SeedOrganisationWithNonAdminPerson(context, orgGuid);

        UserOrganisationMembership? capturedMembership = null;
        _membershipRepositoryMock
            .Setup(r => r.Add(It.IsAny<UserOrganisationMembership>()))
            .Callback<UserOrganisationMembership>(m => capturedMembership = m);

        var service = BuildService(context);

        // Act
        await service.SyncOrganisationMembershipsAsync(orgGuid, orgId);

        // Assert
        capturedMembership.Should().NotBeNull();
        capturedMembership!.OrganisationRole.Should().Be(OrganisationRole.Member);
    }

    [Fact]
    public async Task SyncOrganisationMembershipsAsync_WhenExistingMembersPresent_SecondAdminIsAdminNotOwner()
    {
        // Arrange — org already has one member; next admin sync should get Admin role, not Owner
        var orgGuid = Guid.NewGuid();
        const int orgId = 42;
        var context = CreateContext();
        SeedOrganisationWithAdminPerson(context, orgGuid);

        _membershipRepositoryMock
            .Setup(r => r.GetByOrganisationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new UserOrganisationMembership { Id = 1, OrganisationId = orgId }]);

        UserOrganisationMembership? capturedMembership = null;
        _membershipRepositoryMock
            .Setup(r => r.Add(It.IsAny<UserOrganisationMembership>()))
            .Callback<UserOrganisationMembership>(m => capturedMembership = m);

        var service = BuildService(context);

        // Act
        await service.SyncOrganisationMembershipsAsync(orgGuid, orgId);

        // Assert — existing members exist → admin → Admin not Owner
        capturedMembership.Should().NotBeNull();
        capturedMembership!.OrganisationRole.Should().Be(OrganisationRole.Admin);
    }

    [Fact]
    public async Task SyncOrganisationMembershipsAsync_WhenPersonAlreadyHasMembership_SkipsCreation()
    {
        // Arrange
        var orgGuid = Guid.NewGuid();
        const int orgId = 42;
        var context = CreateContext();
        var person = SeedOrganisationWithAdminPerson(context, orgGuid);

        _membershipRepositoryMock
            .Setup(r => r.GetByPersonIdAndOrganisationAsync(person.Guid, orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserOrganisationMembership { Id = 99, OrganisationId = orgId });

        var service = BuildService(context);

        // Act
        await service.SyncOrganisationMembershipsAsync(orgGuid, orgId);

        // Assert — membership already exists, nothing added
        _membershipRepositoryMock.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncOrganisationMembershipsAsync_WhenPersonHasNoUserUrn_SkipsCreation()
    {
        // Arrange
        var orgGuid = Guid.NewGuid();
        const int orgId = 42;
        var context = CreateContext();
        SeedOrganisationWithPersonNoUrn(context, orgGuid);

        var service = BuildService(context);

        // Act
        await service.SyncOrganisationMembershipsAsync(orgGuid, orgId);

        // Assert — person with no URN → skipped
        _membershipRepositoryMock.Verify(r => r.Add(It.IsAny<UserOrganisationMembership>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private OrganisationPersonsSyncService BuildService(OrganisationInformationContext context) =>
        new(context, _membershipRepositoryMock.Object, _userAssignmentServiceMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);

    private static OrganisationInformationContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrganisationInformationContext>()
            .UseInMemoryDatabase($"org-persons-sync-{Guid.NewGuid()}")
            .Options;
        return new OrganisationInformationContext(options);
    }

    private static void SeedOrganisationWithoutPersons(OrganisationInformationContext context, Guid orgGuid)
    {
        var (org, _) = SeedBase(context, orgGuid);
        context.SaveChanges();
    }

    private static CdpPerson SeedOrganisationWithAdminPerson(OrganisationInformationContext context, Guid orgGuid)
    {
        var (org, person) = SeedBase(context, orgGuid);

        context.Set<OrganisationPerson>().Add(new OrganisationPerson
        {
            Organisation = org,
            Person = person,
            Scopes = ["ADMIN"]
        });

        context.SaveChanges();
        return person;
    }

    private static void SeedOrganisationWithNonAdminPerson(OrganisationInformationContext context, Guid orgGuid)
    {
        var (org, person) = SeedBase(context, orgGuid);

        context.Set<OrganisationPerson>().Add(new OrganisationPerson
        {
            Organisation = org,
            Person = person,
            Scopes = []
        });

        context.SaveChanges();
    }

    private static void SeedOrganisationWithPersonNoUrn(OrganisationInformationContext context, Guid orgGuid)
    {
        var (org, _) = SeedBase(context, orgGuid);
        var personNoUrn = new CdpPerson
        {
            Guid = Guid.NewGuid(),
            FirstName = "No",
            LastName = "Urn",
            Email = "nourn@example.com",
            UserUrn = string.Empty
        };
        context.Persons.Add(personNoUrn);

        context.Set<OrganisationPerson>().Add(new OrganisationPerson
        {
            Organisation = org,
            Person = personNoUrn,
            Scopes = ["ADMIN"]
        });

        context.SaveChanges();
    }

    private static (CdpOrganisation Organisation, CdpPerson Person) SeedBase(
        OrganisationInformationContext context, Guid orgGuid)
    {
        var tenant = new Tenant { Guid = Guid.NewGuid(), Name = "Test Tenant" };
        var org = new CdpOrganisation
        {
            Guid = orgGuid,
            Name = "Test Org",
            Tenant = tenant,
            Type = OrganisationType.Organisation,
            Roles = []
        };
        var person = new CdpPerson
        {
            Guid = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Admin",
            Email = "alice@example.com",
            UserUrn = "urn:fdc:gov.uk:2022:alice"
        };
        context.Tenants.Add(tenant);
        context.Organisations.Add(org);
        context.Persons.Add(person);
        return (org, person);
    }
}
