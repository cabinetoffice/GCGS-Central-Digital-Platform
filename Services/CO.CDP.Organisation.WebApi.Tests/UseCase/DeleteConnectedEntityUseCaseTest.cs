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
    public async Task Execute_ValidConnectedEntity_SetsEndDate_ReturnsTrue()
    {
        var organisation = Organisation();
        var connectedEntity = ConnectedEntity(organisation);
        SetupOrganisationRepository(organisation);
        SetupConnectedEntityRepository(connectedEntity);
        var endDate = DateTimeOffset.Now;
        var deleteConnectedEntity = new DeleteConnectedEntity()
        {
            EndDate = endDate
        };

        var result = await _useCase.Execute((_organisationId, _connectedEntityId, deleteConnectedEntity));

        result.Should().BeTrue();
        connectedEntity.EndDate.Should().Be(endDate);
        _connectedEntityRepository.Verify(repo => repo.Save(connectedEntity), Times.Once);
    }

    [Fact]
    public async Task Execute_InvalidOrganisationId_ThrowsUnknownOrganisationException()
    {
        SetupOrganisationRepository(null);
        SetupConnectedEntityRepository(null);
        var deleteConnectedEntity = new DeleteConnectedEntity() { EndDate = DateTimeOffset.Now };

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _connectedEntityId, deleteConnectedEntity));

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId}.");
    }

    [Fact]
    public async Task Execute_InvalidConnectedEntityId_ThrowsUnknownConnectedEntityException()
    {
        var organisation = Organisation();
        SetupOrganisationRepository(organisation);
        SetupConnectedEntityRepository(null);
        var deleteConnectedEntity = new DeleteConnectedEntity() { EndDate = DateTimeOffset.Now };

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _connectedEntityId, deleteConnectedEntity));

        await act.Should().ThrowAsync<UnknownConnectedEntityException>()
            .WithMessage($"Unknown connected entity {_connectedEntityId}.");
    }

    private Persistence.Organisation Organisation()
    {
        return new Persistence.Organisation { Guid = _organisationId, Name = "Test", Tenant = It.IsAny<Tenant>() };
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