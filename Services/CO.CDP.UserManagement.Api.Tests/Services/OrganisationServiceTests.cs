using CoreEntities = CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class OrganisationServiceTests
{
    private readonly Mock<IOrganisationRepository> _repositoryMock;
    private readonly Mock<ISlugGeneratorService> _slugGeneratorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly OrganisationService _service;

    public OrganisationServiceTests()
    {
        _repositoryMock = new Mock<IOrganisationRepository>();
        _slugGeneratorMock = new Mock<ISlugGeneratorService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new OrganisationService(_repositoryMock.Object, _slugGeneratorMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrganisationExists_ReturnsOrganisation()
    {
        // Arrange
        var organisation = new CoreEntities.Organisation { Id = 1, Name = "Test Org", Slug = "test-org" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(organisation);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Org");
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrganisationDoesNotExist_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((CoreEntities.Organisation?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesOrganisation()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var name = "New Organisation";
        _repositoryMock.Setup(r => r.GetByCdpGuidAsync(cdpGuid, default)).ReturnsAsync((CoreEntities.Organisation?)null);
        _repositoryMock.Setup(r => r.SlugExistsAsync(It.IsAny<string>(), null, default)).ReturnsAsync(false);
        _slugGeneratorMock.Setup(s => s.GenerateSlug(name)).Returns("new-organisation");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(cdpGuid, name, true);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.Slug.Should().Be("new-organisation");
        result.CdpOrganisationGuid.Should().Be(cdpGuid);
        result.IsActive.Should().BeTrue();
        _repositoryMock.Verify(r => r.Add(It.IsAny<CoreEntities.Organisation>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateCdpGuid_ThrowsDuplicateEntityException()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var existingOrg = new CoreEntities.Organisation { Id = 1, CdpOrganisationGuid = cdpGuid, Name = "Existing", Slug = "existing" };
        _repositoryMock.Setup(r => r.GetByCdpGuidAsync(cdpGuid, default)).ReturnsAsync(existingOrg);

        // Act
        var act = async () => await _service.CreateAsync(cdpGuid, "New Name", true);

        // Assert
        await act.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage($"*{nameof(CoreEntities.Organisation.CdpOrganisationGuid)}*");
    }

    [Fact]
    public async Task CreateAsync_WhenSlugExists_GeneratesUniqueSlug()
    {
        // Arrange
        var cdpGuid = Guid.NewGuid();
        var name = "Test Organisation";
        _repositoryMock.Setup(r => r.GetByCdpGuidAsync(cdpGuid, default)).ReturnsAsync((CoreEntities.Organisation?)null);
        _repositoryMock.SetupSequence(r => r.SlugExistsAsync(It.IsAny<string>(), null, default))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        _slugGeneratorMock.Setup(s => s.GenerateSlug(name)).Returns("test-organisation");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(cdpGuid, name, true);

        // Assert
        result.Slug.Should().Be("test-organisation-1");
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesOrganisation()
    {
        // Arrange
        var organisation = new CoreEntities.Organisation { Id = 1, Name = "Old Name", Slug = "old-name", IsActive = true };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(organisation);
        _repositoryMock.Setup(r => r.SlugExistsAsync(It.IsAny<string>(), 1, default)).ReturnsAsync(false);
        _slugGeneratorMock.Setup(s => s.GenerateSlug("New Name")).Returns("new-name");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateAsync(1, "New Name", false);

        // Assert
        result.Name.Should().Be("New Name");
        result.Slug.Should().Be("new-name");
        result.IsActive.Should().BeFalse();
        _repositoryMock.Verify(r => r.Update(organisation), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenOrganisationDoesNotExist_ThrowsEntityNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((CoreEntities.Organisation?)null);

        // Act
        var act = async () => await _service.UpdateAsync(999, "New Name", true);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage("*Organisation*999*");
    }

    [Fact]
    public async Task DeleteAsync_WhenOrganisationExists_DeletesOrganisation()
    {
        // Arrange
        var organisation = new CoreEntities.Organisation { Id = 1, Name = "Test", Slug = "test" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(organisation);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _repositoryMock.Verify(r => r.Remove(organisation), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenOrganisationDoesNotExist_ThrowsEntityNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((CoreEntities.Organisation?)null);

        // Act
        var act = async () => await _service.DeleteAsync(999);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
