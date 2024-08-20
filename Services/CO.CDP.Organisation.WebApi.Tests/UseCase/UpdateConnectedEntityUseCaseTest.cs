using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateConnectedEntityUseCaseTests
{
    private readonly Mock<Persistence.IConnectedEntityRepository> _mockConnectedEntityRepository;
    private readonly Mock<Persistence.IOrganisationRepository> _mockOrganisationRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UpdateConnectedEntityUseCase _useCase;

    public UpdateConnectedEntityUseCaseTests()
    {
        _mockConnectedEntityRepository = new Mock<Persistence.IConnectedEntityRepository>();
        _mockOrganisationRepository = new Mock<Persistence.IOrganisationRepository>();
        _mockMapper = new Mock<IMapper>();

        _useCase = new UpdateConnectedEntityUseCase(
            _mockConnectedEntityRepository.Object,
            _mockOrganisationRepository.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Execute_ThrowsUnknownOrganisationException_WhenOrganisationDoesNotExist()
    {
        var command = (organisationId: Guid.NewGuid(), connectedEntityId: Guid.NewGuid(), updateConnectedEntity: new UpdateConnectedEntity
        {
            EntityType = (ConnectedEntityType)0
        });
        _mockOrganisationRepository.Setup(repo => repo.Find(command.organisationId)).ReturnsAsync((Persistence.Organisation?)null);

        await Assert.ThrowsAsync<UnknownOrganisationException>(() => _useCase.Execute(command));
    }

    private static Persistence.Organisation FakeOrganisation(bool? withSupplierInfo = true)
    {
        Persistence.Organisation org = new()
        {
            Guid = Guid.NewGuid(),
            Name = "FakeOrg",
            Tenant = new Persistence.Tenant()
            {
                Guid = Guid.NewGuid(),
                Name = "Tenant 101"
            },
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "contact@test.org" }]
        };

        if (withSupplierInfo == true)
        {
            org.SupplierInfo = new Persistence.Organisation.SupplierInformation { CompletedRegAddress = true };
        }

        return org;
    }

    private static Persistence.ConnectedEntity FakeConnectedEntity(Persistence.Organisation organisation)
    {
        Persistence.ConnectedEntity connectedEntity = new()
        {
            Guid = Guid.NewGuid(),
            EntityType = Persistence.ConnectedEntity.ConnectedEntityType.Individual,
            SupplierOrganisation = organisation,
            Addresses = new List<Persistence.ConnectedEntity.ConnectedEntityAddress>(),
            Id = 1,
            HasCompnayHouseNumber = true,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        return connectedEntity;
    }

    [Fact]
    public async Task Execute_ThrowsUnknownConnectedEntityException_WhenConnectedEntityDoesNotExist()
    {
        var organisationId = Guid.NewGuid();
        var connectedEntityId = Guid.NewGuid();
        var updateConnectedEntity = new UpdateConnectedEntity
        {
            EntityType = (ConnectedEntityType)0
        };
        var command = (organisationId, connectedEntityId, updateConnectedEntity);

        var organisation = FakeOrganisation();

        _mockOrganisationRepository.Setup(repo => repo.Find(organisationId)).ReturnsAsync(organisation);
        _mockConnectedEntityRepository.Setup(repo => repo.Find(organisationId, connectedEntityId)).ReturnsAsync((Persistence.ConnectedEntity?)null);

        await Assert.ThrowsAsync<UnknownConnectedEntityException>(() => _useCase.Execute(command));
    }

    [Fact]
    public async Task Execute_MapsUpdateRequestToConnectedEntity()
    {
        var organisationId = Guid.NewGuid();
        var connectedEntityId = Guid.NewGuid();
        var updateConnectedEntity = new UpdateConnectedEntity
        {
            EntityType = (ConnectedEntityType)0
        };
        var command = (organisationId, connectedEntityId, updateConnectedEntity);

        var organisation = FakeOrganisation();
        var connectedEntity = FakeConnectedEntity(organisation);

        _mockOrganisationRepository.Setup(repo => repo.Find(organisationId)).ReturnsAsync(organisation);
        _mockConnectedEntityRepository.Setup(repo => repo.Find(organisationId, connectedEntityId)).ReturnsAsync(connectedEntity);
        _mockMapper.Setup(m => m.Map(updateConnectedEntity, connectedEntity)).Returns(connectedEntity);

        await _useCase.Execute(command);

        _mockMapper.Verify(m => m.Map(updateConnectedEntity, connectedEntity), Times.Once);
    }

    [Fact]
    public async Task Execute_SavesConnectedEntity()
    {
        var organisationId = Guid.NewGuid();
        var connectedEntityId = Guid.NewGuid();
        var updateConnectedEntity = new UpdateConnectedEntity
        {
            EntityType = (ConnectedEntityType)0
        };
        var command = (organisationId, connectedEntityId, updateConnectedEntity);

        var organisation = FakeOrganisation();
        var connectedEntity = FakeConnectedEntity(organisation);

        _mockOrganisationRepository.Setup(repo => repo.Find(organisationId)).ReturnsAsync(organisation);
        _mockConnectedEntityRepository.Setup(repo => repo.Find(organisationId, connectedEntityId)).ReturnsAsync(connectedEntity);
        _mockMapper.Setup(m => m.Map(updateConnectedEntity, connectedEntity)).Returns(connectedEntity);

        await _useCase.Execute(command);

        _mockConnectedEntityRepository.Verify(repo => repo.Save(It.IsAny<Persistence.ConnectedEntity>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ReturnsTrue_WhenOperationIsSuccessful()
    {
        var organisationId = Guid.NewGuid();
        var connectedEntityId = Guid.NewGuid();
        var updateConnectedEntity = new UpdateConnectedEntity
        {
            EntityType = (ConnectedEntityType)0
        };
        var command = (organisationId, connectedEntityId, updateConnectedEntity);

        var organisation = FakeOrganisation();
        var connectedEntity = FakeConnectedEntity(organisation);

        _mockOrganisationRepository.Setup(repo => repo.Find(organisationId)).ReturnsAsync(organisation);
        _mockConnectedEntityRepository.Setup(repo => repo.Find(organisationId, connectedEntityId)).ReturnsAsync(connectedEntity);
        _mockMapper.Setup(m => m.Map(updateConnectedEntity, connectedEntity)).Returns(connectedEntity);

        var result = await _useCase.Execute(command);

        Assert.True(result);
    }
}
