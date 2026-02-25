using CO.CDP.OrganisationInformation;
using CO.CDP.UserManagement.Infrastructure.Events;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Subscribers;

public class OrganisationSyncServiceRegisteredTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly Mock<ISlugGeneratorService> _slugGeneratorMock;
    private readonly Mock<IOrganisationPersonsSyncService> _personsSyncServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly OrganisationSyncService _service;

    public OrganisationSyncServiceRegisteredTests()
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _slugGeneratorMock = new Mock<ISlugGeneratorService>();
        _personsSyncServiceMock = new Mock<IOrganisationPersonsSyncService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<OrganisationSyncService>>();

        _personsSyncServiceMock
            .Setup(s => s.SyncOrganisationMembershipsAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _service = new OrganisationSyncService(
            _organisationRepositoryMock.Object,
            _slugGeneratorMock.Object,
            _personsSyncServiceMock.Object,
            _unitOfWorkMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenOrgNotExists_CreatesOrganisation()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var @event = CreateOrganisationRegisteredEvent(cdpGuid.ToString(), "Test Organisation");

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpGuid, default))
            .ReturnsAsync((CoreOrganisation?)null);

        _slugGeneratorMock
            .Setup(s => s.GenerateSlug("Test Organisation"))
            .Returns("test-organisation");

        _organisationRepositoryMock
            .Setup(r => r.SlugExistsAsync("test-organisation", null, default))
            .ReturnsAsync(false);

        // Act
        await _service.SyncRegisteredAsync(@event.Id, @event.Name);

        // Assert
        _organisationRepositoryMock.Verify(r => r.Add(It.Is<CoreOrganisation>(o =>
            o.CdpOrganisationGuid == cdpGuid &&
            o.Name == "Test Organisation" &&
            o.Slug == "test-organisation" &&
            o.IsActive &&
            o.CreatedBy == "system:org-sync"
        )), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrgAlreadyExists_LogsInfoAndSkips()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var @event = CreateOrganisationRegisteredEvent(cdpGuid.ToString(), "Existing Organisation");

        var existingOrg = new CoreOrganisation
        {
            Id = 123,
            CdpOrganisationGuid = cdpGuid,
            Name = "Existing Organisation",
            Slug = "existing-organisation"
        };

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpGuid, default))
            .ReturnsAsync(existingOrg);

        // Act
        await _service.SyncRegisteredAsync(@event.Id, @event.Name);

        // Assert
        _organisationRepositoryMock.Verify(r => r.Add(It.IsAny<CoreOrganisation>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_SetsCorrectSlugFromName()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var @event = CreateOrganisationRegisteredEvent(cdpGuid.ToString(), "Acme Corporation Ltd");

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpGuid, default))
            .ReturnsAsync((CoreOrganisation?)null);

        _slugGeneratorMock
            .Setup(s => s.GenerateSlug("Acme Corporation Ltd"))
            .Returns("acme-corporation-ltd");

        _organisationRepositoryMock
            .Setup(r => r.SlugExistsAsync("acme-corporation-ltd", null, default))
            .ReturnsAsync(false);

        // Act
        await _service.SyncRegisteredAsync(@event.Id, @event.Name);

        // Assert
        _organisationRepositoryMock.Verify(r => r.Add(It.Is<CoreOrganisation>(o =>
            o.Slug == "acme-corporation-ltd"
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSlugCollision_AppendsCounter()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var @event = CreateOrganisationRegisteredEvent(cdpGuid.ToString(), "Test Org");

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpGuid, default))
            .ReturnsAsync((CoreOrganisation?)null);

        _slugGeneratorMock
            .Setup(s => s.GenerateSlug("Test Org"))
            .Returns("test-org");

        var attemptCount = 0;
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(() =>
            {
                attemptCount++;
                if (attemptCount <= 2)
                {
                    // First two attempts fail with unique constraint violation
                    var pgException = new Npgsql.PostgresException("", "", "", "23505");
                    throw new Microsoft.EntityFrameworkCore.DbUpdateException("", pgException);
                }
                return 1; // Third attempt succeeds
            });

        // Act
        await _service.SyncRegisteredAsync(@event.Id, @event.Name);

        // Assert
        _organisationRepositoryMock.Verify(r => r.Add(It.Is<CoreOrganisation>(o =>
            o.Slug == "test-org-2"
        )), Times.Once);

        Assert.Equal(3, attemptCount); // Verify it tried 3 times
    }

    [Fact]
    public async Task Handle_WhenGuidFormatInvalid_ThrowsArgumentException()
    {
        // Arrange
        var @event = CreateOrganisationRegisteredEvent("not-a-valid-guid", "Test Org");

        // Act
        var act = async () => await _service.SyncRegisteredAsync(@event.Id, @event.Name);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid GUID format: not-a-valid-guid");

        _organisationRepositoryMock.Verify(r => r.Add(It.IsAny<CoreOrganisation>()), Times.Never);
    }

    private static OrganisationRegistered CreateOrganisationRegisteredEvent(string id, string name)
    {
        return new OrganisationRegistered
        {
            Id = id,
            Name = name,
            Identifier = new Identifier
            {
                Id = "12345",
                LegalName = name,
                Scheme = "GB-COH"
            },
            ContactPoint = new ContactPoint
            {
                Name = "Contact",
                Email = "contact@example.com"
            },
            Roles = new List<string> { "tenderer" },
            Type = OrganisationType.Organisation
        };
    }
}
