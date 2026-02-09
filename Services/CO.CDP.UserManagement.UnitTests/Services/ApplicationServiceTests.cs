using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.UnitTests.Services;

public class ApplicationServiceTests
{
    private readonly Mock<IApplicationRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ApplicationService _service;

    public ApplicationServiceTests()
    {
        _repositoryMock = new Mock<IApplicationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new ApplicationService(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenApplicationExists_ReturnsApplication()
    {
        // Arrange
        var application = new Application { Id = 1, Name = "Test App", ClientId = "test-app" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(application);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test App");
    }

    [Fact]
    public async Task GetByClientIdAsync_WhenApplicationExists_ReturnsApplication()
    {
        // Arrange
        var application = new Application { Id = 1, Name = "Test App", ClientId = "test-app" };
        _repositoryMock.Setup(r => r.GetByClientIdAsync("test-app", default)).ReturnsAsync(application);

        // Act
        var result = await _service.GetByClientIdAsync("test-app");

        // Assert
        result.Should().NotBeNull();
        result!.ClientId.Should().Be("test-app");
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesApplication()
    {
        // Arrange
        var clientId = "new-app-client";
        var name = "New Application";
        _repositoryMock.Setup(r => r.ClientIdExistsAsync(clientId, null, default)).ReturnsAsync(false);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(name, clientId, "Description", true);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.ClientId.Should().Be(clientId);
        result.Description.Should().Be("Description");
        result.IsActive.Should().BeTrue();
        _repositoryMock.Verify(r => r.Add(It.IsAny<Application>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateClientId_ThrowsDuplicateEntityException()
    {
        // Arrange
        var clientId = "existing-client-id";
        _repositoryMock.Setup(r => r.ClientIdExistsAsync(clientId, null, default)).ReturnsAsync(true);

        // Act
        var act = async () => await _service.CreateAsync("Name", clientId, null, true);

        // Assert
        await act.Should().ThrowAsync<DuplicateEntityException>()
            .WithMessage($"*{nameof(Application.ClientId)}*{clientId}*");
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesApplication()
    {
        // Arrange
        var application = new Application { Id = 1, Name = "Old Name", ClientId = "app-client", IsActive = true };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(application);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateAsync(1, "New Name", "New Description", false);

        // Assert
        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New Description");
        result.IsActive.Should().BeFalse();
        _repositoryMock.Verify(r => r.Update(application), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenApplicationDoesNotExist_ThrowsEntityNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((Application?)null);

        // Act
        var act = async () => await _service.UpdateAsync(999, "Name", "Description", true);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage("*Application*999*");
    }

    [Fact]
    public async Task DeleteAsync_WhenApplicationExists_DeletesApplication()
    {
        // Arrange
        var application = new Application { Id = 1, Name = "Test", ClientId = "test" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(application);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _repositoryMock.Verify(r => r.Remove(application), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
