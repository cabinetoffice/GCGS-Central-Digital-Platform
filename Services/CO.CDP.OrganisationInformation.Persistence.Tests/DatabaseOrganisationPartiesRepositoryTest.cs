using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseOrganisationPartiesRepositoryTest(OrganisationInformationPostgreSqlFixture postgreSql)
    : IClassFixture<OrganisationInformationPostgreSqlFixture>
{
    [Fact]
    public async Task Find_ShouldReturnOrganisationParties_WhenEntitiesExist()
    {
        var parentOrg = GivenOrganisation(name: "Parent Organisation");
        var childOrg1 = GivenOrganisation(name: "Child Organisation 1");
        var childOrg2 = GivenOrganisation(name: "Child Organisation 2");
        using var repository = OrganisationRepository();
        repository.Save(parentOrg);
        repository.Save(childOrg1);
        repository.Save(childOrg2);

        using var repositoryParties = OrganisationPartiesRepository();
        await repositoryParties.Save(new OrganisationParty
        {
            ParentOrganisationId = parentOrg.Id,
            ChildOrganisationId = childOrg1.Id,
            OrganisationRelationship = OrganisationRelationship.Consortium
        });

        await repositoryParties.Save(new OrganisationParty
        {
            ParentOrganisationId = parentOrg.Id,
            ChildOrganisationId = childOrg2.Id,
            OrganisationRelationship = OrganisationRelationship.Consortium
        });

        var organisationParties = await repositoryParties.Find(parentOrg.Guid);

        organisationParties.Should().NotBeNull();
        organisationParties.Should().HaveCount(2);
        organisationParties.As<IEnumerable<OrganisationParty>>()
            .Select(p => p.ChildOrganisationId).Should().Contain([childOrg1.Id, childOrg2.Id]);
    }

    private DatabaseOrganisationPartiesRepository OrganisationPartiesRepository()
       => new(GetDbContext());

    private DatabaseOrganisationRepository OrganisationRepository()
        => new(GetDbContext());

    private OrganisationInformationContext? context = null;

    private OrganisationInformationContext GetDbContext()
    {
        context = context ?? postgreSql.OrganisationInformationContext();

        return context;
    }
}