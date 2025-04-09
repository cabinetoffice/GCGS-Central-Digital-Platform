using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class DeleteConnectedEntityUseCaseTest
{
    private readonly Mock<IOrganisationRepository> _organisationRepository;
    private readonly Mock<IConnectedEntityRepository> _connectedEntityRepository;
    private readonly DeleteConnectedEntityUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _connectedEntityId = Guid.NewGuid();

    public DeleteConnectedEntityUseCaseTest()
    {
        _organisationRepository = new Mock<IOrganisationRepository>();
        _connectedEntityRepository = new Mock<IConnectedEntityRepository>();
        _useCase = new DeleteConnectedEntityUseCase(_organisationRepository.Object, _connectedEntityRepository.Object);
    }

    [Fact]
    public async Task Execute_ValidConnectedEntityNotInUse_SetsDeleted_ReturnsTrue()
    {
        var organisation = Organisation();
        var connectedEntity = ConnectedEntity(organisation);
        SetupOrganisationRepository(organisation);
        SetupConnectedEntityRepository(connectedEntity);

        _connectedEntityRepository
            .Setup(ce => ce.IsConnectedEntityUsedInExclusionAsync(_organisationId, _connectedEntityId))
            .Returns(Task.FromResult(new Tuple<bool, Guid, Guid>(false, Guid.Empty, Guid.Empty)));

        var result = await _useCase.Execute((_organisationId, _connectedEntityId));

        result.Success.Should().BeTrue();
        connectedEntity.Deleted.Should().BeTrue();
        _connectedEntityRepository.Verify(repo => repo.Save(connectedEntity), Times.Once);
    }

    [Fact]
    public async Task Execute_ValidConnectedEntityInUse_SetsDeleted_ReturnsTrue()
    {
        var organisation = Organisation();
        var connectedEntity = ConnectedEntity(organisation);
        SetupOrganisationRepository(organisation);
        SetupConnectedEntityRepository(connectedEntity);

        _connectedEntityRepository
            .Setup(ce => ce.IsConnectedEntityUsedInExclusionAsync(_organisationId, _connectedEntityId))
            .Returns(Task.FromResult(new Tuple<bool, Guid, Guid>(true, Guid.Empty, Guid.Empty)));

        var result = await _useCase.Execute((_organisationId, _connectedEntityId));

        result.Success.Should().BeFalse();
        connectedEntity.Deleted.Should().BeFalse();
        _connectedEntityRepository.Verify(repo => repo.Save(connectedEntity), Times.Never);
    }

    [Fact]
    public async Task Execute_InvalidOrganisationId_ThrowsUnknownOrganisationException()
    {
        SetupOrganisationRepository(null);
        SetupConnectedEntityRepository(null);
        
        Func<Task> act = async () => await _useCase.Execute((_organisationId, _connectedEntityId));

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId}.");
    }

    [Fact]
    public async Task Execute_InvalidConnectedEntityId_ThrowsUnknownConnectedEntityException()
    {
        var organisation = Organisation();
        SetupOrganisationRepository(organisation);
        SetupConnectedEntityRepository(null);
        
        Func<Task> act = async () => await _useCase.Execute((_organisationId, _connectedEntityId));

        await act.Should().ThrowAsync<UnknownConnectedEntityException>()
            .WithMessage($"Unknown connected entity {_connectedEntityId}.");
    }

    private Persistence.Organisation Organisation()
    {
        return new Persistence.Organisation
        {
            Guid = _organisationId,
            Name = "Test",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>()
        };
    }

    private Persistence.ConnectedEntity ConnectedEntity(Persistence.Organisation organisation)
    {
        return new Persistence.ConnectedEntity() { Guid = _connectedEntityId, EntityType = Persistence.ConnectedEntity.ConnectedEntityType.Organisation, SupplierOrganisation = organisation };
    }

    private void SetupOrganisationRepository(Persistence.Organisation? organisation)
        => _organisationRepository.Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(organisation);

    private void SetupConnectedEntityRepository(Persistence.ConnectedEntity? connectedEntity)
        => _connectedEntityRepository.Setup(repo => repo.Find(_organisationId, _connectedEntityId))
            .ReturnsAsync(connectedEntity);
}