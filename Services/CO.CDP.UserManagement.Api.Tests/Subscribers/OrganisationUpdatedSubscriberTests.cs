using CO.CDP.OrganisationInformation;
using CO.CDP.UserManagement.Infrastructure.Events;
using CO.CDP.UserManagement.Infrastructure.Subscribers;
using CO.CDP.UserManagement.Core.Interfaces;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Subscribers;

public class OrganisationUpdatedSubscriberTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly Mock<ISlugGeneratorService> _slugGeneratorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly OrganisationUpdatedSubscriber _subscriber;

    public OrganisationUpdatedSubscriberTests()
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _slugGeneratorMock = new Mock<ISlugGeneratorService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<OrganisationUpdatedSubscriber>>();

        _subscriber = new OrganisationUpdatedSubscriber(
            _organisationRepositoryMock.Object,
            _slugGeneratorMock.Object,
            _unitOfWorkMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenOrgExists_UpdatesNameAndSlug()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var @event = CreateOrganisationUpdatedEvent(cdpGuid.ToString(), "Updated Name");

        var existingOrg = new UmOrganisation
        {
            Id = 123,
            CdpOrganisationGuid = cdpGuid,
            Name = "Old Name",
            Slug = "old-name",
            IsActive = true
        };

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpGuid, default))
            .ReturnsAsync(existingOrg);

        _slugGeneratorMock
            .Setup(s => s.GenerateSlug("Updated Name"))
            .Returns("updated-name");

        _organisationRepositoryMock
            .Setup(r => r.SlugExistsAsync("updated-name", 123, default))
            .ReturnsAsync(false);

        // Act
        await _subscriber.Handle(@event);

        // Assert
        existingOrg.Name.Should().Be("Updated Name");
        existingOrg.Slug.Should().Be("updated-name");
        existingOrg.ModifiedBy.Should().Be("system:org-sync");

        _organisationRepositoryMock.Verify(r => r.Update(existingOrg), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrgExists_AndNameUnchanged_DoesNotUpdate()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var @event = CreateOrganisationUpdatedEvent(cdpGuid.ToString(), "Same Name");

        var existingOrg = new UmOrganisation
        {
            Id = 123,
            CdpOrganisationGuid = cdpGuid,
            Name = "Same Name",
            Slug = "same-name",
            IsActive = true
        };

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpGuid, default))
            .ReturnsAsync(existingOrg);

        // Act
        await _subscriber.Handle(@event);

        // Assert
        _organisationRepositoryMock.Verify(r => r.Update(It.IsAny<UmOrganisation>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrgNotExists_CreatesOrg()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var @event = CreateOrganisationUpdatedEvent(cdpGuid.ToString(), "New Org");

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpGuid, default))
            .ReturnsAsync((UmOrganisation?)null);

        _slugGeneratorMock
            .Setup(s => s.GenerateSlug("New Org"))
            .Returns("new-org");

        _organisationRepositoryMock
            .Setup(r => r.SlugExistsAsync("new-org", null, default))
            .ReturnsAsync(false);

        // Act
        await _subscriber.Handle(@event);

        // Assert
        _organisationRepositoryMock.Verify(r => r.Add(It.Is<UmOrganisation>(o =>
            o.CdpOrganisationGuid == cdpGuid &&
            o.Name == "New Org" &&
            o.Slug == "new-org" &&
            o.IsActive &&
            o.CreatedBy == "system:org-sync"
        )), Times.Once);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSlugCollisionOnUpdate_AppendsCounter()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var @event = CreateOrganisationUpdatedEvent(cdpGuid.ToString(), "Common Name");

        var existingOrg = new UmOrganisation
        {
            Id = 123,
            CdpOrganisationGuid = cdpGuid,
            Name = "Old Name",
            Slug = "old-name",
            IsActive = true
        };

        _organisationRepositoryMock
            .Setup(r => r.GetByCdpGuidAsync(cdpGuid, default))
            .ReturnsAsync(existingOrg);

        _slugGeneratorMock
            .Setup(s => s.GenerateSlug("Common Name"))
            .Returns("common-name");

        var attemptCount = 0;
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(() =>
            {
                attemptCount++;
                if (attemptCount == 1)
                {
                    // First attempt fails with unique constraint violation
                    var pgException = new Npgsql.PostgresException("", "", "", "23505");
                    throw new Microsoft.EntityFrameworkCore.DbUpdateException("", pgException);
                }
                return 1; // Second attempt succeeds
            });

        // Act
        await _subscriber.Handle(@event);

        // Assert
        existingOrg.Slug.Should().Be("common-name-1");
        _organisationRepositoryMock.Verify(r => r.Update(existingOrg), Times.Exactly(2));
        Assert.Equal(2, attemptCount); // Verify it tried twice
    }

    [Fact]
    public async Task Handle_WhenGuidFormatInvalid_ThrowsArgumentException()
    {
        // Arrange
        var @event = CreateOrganisationUpdatedEvent("invalid-guid", "Test");

        // Act
        var act = async () => await _subscriber.Handle(@event);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid GUID format: invalid-guid");

        _organisationRepositoryMock.Verify(r => r.Update(It.IsAny<UmOrganisation>()), Times.Never);
        _organisationRepositoryMock.Verify(r => r.Add(It.IsAny<UmOrganisation>()), Times.Never);
    }

    private static OrganisationUpdated CreateOrganisationUpdatedEvent(string id, string name)
    {
        return new OrganisationUpdated
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
            Roles = new List<string> { "tenderer" }
        };
    }
}
