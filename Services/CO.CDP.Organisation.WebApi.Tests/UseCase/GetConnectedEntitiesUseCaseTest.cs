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
        var formGuid = Guid.NewGuid();
        var sectionGuid = Guid.NewGuid();

        List<ConnectedEntityLookup> persistenceConnectedEntityList =
        [
            new ConnectedEntityLookup
            {
                EntityId = eid1,
                Name = "CHN_123",
                EntityType = OrganisationInformation.ConnectedEntityType.Organisation
            },
            new ConnectedEntityLookup
            {
                EntityId = eid2,
                Name = "First Name",
                EntityType = OrganisationInformation.ConnectedEntityType.Organisation
            }
        ];


        _repository.Setup(r => r.GetSummary(organisationId)).ReturnsAsync(persistenceConnectedEntityList);
        _repository
            .Setup(r => r.IsConnectedEntityUsedInExclusionAsync(organisationId, It.IsAny<Guid>()))
            .ReturnsAsync(Tuple.Create(false, formGuid, sectionGuid));

        var found = await UseCase.Execute(organisationId);

        var expectedModelConnectedEntity = new List<Model.ConnectedEntityLookup>
        {
            new Model.ConnectedEntityLookup
            {
                EntityId = eid1,
                Name = "CHN_123",
                Uri = new Uri($"https://cdp.cabinetoffice.gov.uk/organisations/{organisationId}/connected-entities/{eid1}"),
                EntityType = Model.ConnectedEntityType.Organisation,
                FormGuid = formGuid,
                SectionGuid = sectionGuid,
            },
            new Model.ConnectedEntityLookup
            {
                EntityId = eid2,
                Name = "First Name",
                Uri = new Uri($"https://cdp.cabinetoffice.gov.uk/organisations/{organisationId}/connected-entities/{eid2}"),
                EntityType = Model.ConnectedEntityType.Organisation,
                FormGuid = formGuid,
                SectionGuid = sectionGuid,
            }
        };

        found.Should().BeEquivalentTo(expectedModelConnectedEntity, options => options.ComparingByMembers<Model.ConnectedEntityLookup>());
    }
}