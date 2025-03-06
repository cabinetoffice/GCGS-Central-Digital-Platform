using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetOrganisationPartiesUseCaseTests(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationPartiesRepository> _orgPartiesRepoMock = new();
    private GetOrganisationPartiesUseCase UseCase => new(_orgPartiesRepoMock.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_ShouldReturnNull_WhenNoPartiesFound()
    {
        var organisationId = Guid.NewGuid();
        _orgPartiesRepoMock.Setup(repo => repo.Find(organisationId)).ReturnsAsync([]);

        var result = await UseCase.Execute(organisationId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Execute_ShouldReturnMappedParties_WhenPartiesAreFound()
    {
        var organisationId = Guid.NewGuid();
        var parentOrg = GivenOrganisation(organisationId);
        var childOrg1 = GivenOrganisation(Guid.NewGuid());
        var childOrg2 = GivenOrganisation(Guid.NewGuid());


        var persistenceParties = new List<Persistence.OrganisationParty>
        {
            new() {
                    Id = 1,
                    ParentOrganisationId = parentOrg.Id,
                    ChildOrganisationId = childOrg1.Id,
                    ChildOrganisation =childOrg1,
                    OrganisationRelationship = Persistence.OrganisationRelationship.Consortium,
                },

            new() {
                    Id = 1,
                    ParentOrganisationId = parentOrg.Id,
                    ChildOrganisationId = childOrg2.Id,
                    ChildOrganisation = childOrg2,
                    OrganisationRelationship = Persistence.OrganisationRelationship.Consortium,
                },
        };

        _orgPartiesRepoMock.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(persistenceParties);

        var result = await UseCase.Execute(organisationId);

        result.Should().NotBeNull();
        result.As<OrganisationParties>().Parties.Should().HaveCount(2);
        _orgPartiesRepoMock.Verify(repo => repo.Find(organisationId), Times.Once);
    }

    private static Persistence.Organisation GivenOrganisation(Guid guid) =>
        new()
        {
            Guid = guid,
            Name = "Test",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.SupplierInformation()
        };
}