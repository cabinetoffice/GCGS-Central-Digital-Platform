using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class GetConnectedEntitiesUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IConnectedEntityRepository> _repository = new();
    private GetConnectedEntitiesUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsEmptyIfNoConnectedEntityIsFound()
    {
        var organisationId = Guid.NewGuid();

        var found = await UseCase.Execute(organisationId);

        found.Should().BeEmpty();
    }

    [Fact]
    public async Task ItReturnsTheFoundConnectedEntities()
    {
        var organisationId = Guid.NewGuid();
        var eid1 = Guid.NewGuid();
        var eid2 = Guid.NewGuid();

        var persistenceConnectedEntityList = new List<OrganisationInformation.Persistence.ConnectedEntityLookup>
        {
            new OrganisationInformation.Persistence.ConnectedEntityLookup
            {
                EntityId = eid1,
                Name = "CHN_123",
                EntityType = ConnectedEntity.ConnectedEntityType.Organisation
            },
            new OrganisationInformation.Persistence.ConnectedEntityLookup
            {
                EntityId = eid2,
                Name = "First Name",
                EntityType = ConnectedEntity.ConnectedEntityType.Organisation
            }
        };


        _repository.Setup(r => r.GetSummary(organisationId)).ReturnsAsync(persistenceConnectedEntityList);

        var found = await UseCase.Execute(organisationId);

        var expectedModelConnectedEntity = new List<Model.ConnectedEntityLookup>
        {
            new Model.ConnectedEntityLookup
            {
                EntityId = eid1,
                Name = "CHN_123",
                Uri = new Uri($"https://cdp.cabinetoffice.gov.uk/organisations/{organisationId}/connected-entities/{eid1}"),
                EntityType = Model.ConnectedEntityType.Organisation
            },
            new Model.ConnectedEntityLookup
            {
                EntityId = eid2,
                Name = "First Name",
                Uri = new Uri($"https://cdp.cabinetoffice.gov.uk/organisations/{organisationId}/connected-entities/{eid2}"),
                EntityType = Model.ConnectedEntityType.Organisation
            }
        };

        found.Should().BeEquivalentTo(expectedModelConnectedEntity, options => options.ComparingByMembers<Model.ConnectedEntityLookup>());
    }
}