using CO.CDP.Organisation.WebApi;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class GetConnectedEntityUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IConnectedEntityRepository> _repository = new();
    private GetConnectedEntityUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsEmptyIfNoConnectedEntityIsFound()
    {
        var entityId = Guid.NewGuid();

        var found = await UseCase.Execute(entityId);

        found.Should().BeNull();
    }

    [Fact]
    public async Task ItReturnsTheFoundConnectedEntities()
    {
        var organisationId = Guid.NewGuid();
        var eid1 = Guid.NewGuid();

        var persistenceConnectedEntity = new OrganisationInformation.Persistence.ConnectedEntity
        {
            EntityType = ConnectedEntity.ConnectedEntityType.Organisation,
            Guid = eid1,
            SupplierOrganisation = It.IsAny<OrganisationInformation.Persistence.Organisation>(),
            Organisation = It.IsAny<OrganisationInformation.Persistence.ConnectedEntity.ConnectedOrganisation>()            
        };

        _repository.Setup(r => r.Find(organisationId)).ReturnsAsync(persistenceConnectedEntity);

        var found = await UseCase.Execute(organisationId);

        var expectedModelConnectedEntity = new Model.ConnectedEntity
        {
            Id = eid1,
            EntityType = Model.ConnectedEntityType.Organisation,
            Organisation = It.IsAny<Model.ConnectedOrganisation>()
        };

        found.Should().BeEquivalentTo(expectedModelConnectedEntity, options => options.ComparingByMembers<Model.ConnectedEntityLookup>());
    }
}