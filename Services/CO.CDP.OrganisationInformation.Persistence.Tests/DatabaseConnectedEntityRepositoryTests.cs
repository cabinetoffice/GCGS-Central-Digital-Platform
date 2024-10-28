using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseConnectedEntityRepositoryTests(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItReturnsNullIfConnectedEntityIsNotFound()
    {
        using var repository = ConnectedEntityRepository();

        var found = await repository.Find(Guid.NewGuid(), Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public async Task Find_ShouldReturnConnectedEntity_WhenEntityExists()
    {
        using var repositoryCE = ConnectedEntityRepository();
        using var repositoryOrg = OrganisationRepository();
        var guid = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var person = GivenPerson();
        var organisation = GivenOrganisation(guid: orgId, personsWithScope: [(person, ["ADMIN"])]);

        repositoryOrg.Save(organisation);

        var expectedEntity = new ConnectedEntity
        {
            Guid = guid,
            EntityType = ConnectedEntity.ConnectedEntityType.Organisation,
            Organisation = new ConnectedEntity.ConnectedOrganisation
            {
                OrganisationId = orgId,
                Name = "CHN_111",
                Category = ConnectedEntity.ConnectedOrganisationCategory.DirectorOrTheSameResponsibilities,
                RegisteredLegalForm = "Legal Form",
                LawRegistered = "Law Registered"
            },
            SupplierOrganisation = organisation
        };

        await repositoryCE.Save(expectedEntity);

        var result = await repositoryCE.Find(orgId, guid);

        result.Should().BeEquivalentTo(expectedEntity);
    }

    [Fact]
    public async Task GetSummary_ShouldReturnConnectedEntityLookups_WhenEntitiesExist()
    {
        var organisationId = Guid.NewGuid();
        var guid = Guid.NewGuid();
        using var repositoryCE = ConnectedEntityRepository();
        using var repositoryOrg = OrganisationRepository();
        var person = GivenPerson();
        var organisation = GivenOrganisation(guid: organisationId, personsWithScope: [(person, ["ADMIN"])]);

        repositoryOrg.Save(organisation);
        var ce1 = new ConnectedEntity
        {
            Guid = guid,
            EntityType = ConnectedEntity.ConnectedEntityType.Organisation,
            Organisation = new ConnectedEntity.ConnectedOrganisation
            {
                OrganisationId = organisationId,
                Name = "CHN_111",
                Category = ConnectedEntity.ConnectedOrganisationCategory.DirectorOrTheSameResponsibilities,
                RegisteredLegalForm = "Legal Form",
                LawRegistered = "Law Registered"
            },
            SupplierOrganisation = organisation
        };
        var ce2 = new ConnectedEntity
        {
            Guid = Guid.NewGuid(),
            EntityType = ConnectedEntity.ConnectedEntityType.TrustOrTrustee,
            IndividualOrTrust = new ConnectedEntity.ConnectedIndividualTrust
            {
                FirstName = "First Name",
                LastName = "Last Name",
                Category = ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForIndiv,
                ConnectedType = ConnectedEntity.ConnectedPersonType.Individual
            },
            SupplierOrganisation = organisation
        };

        var connectedEntities = new List<ConnectedEntity>() { ce1, ce2 };

        await repositoryCE.Save(ce1);
        await repositoryCE.Save(ce2);

        var result = await repositoryCE.GetSummary(organisationId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(x => x!.Name == "CHN_111");
        result.Should().ContainSingle(x => x!.Name == "First Name");
    }

    private IConnectedEntityRepository ConnectedEntityRepository()
    {
        return new DatabaseConnectedEntityRepository(postgreSql.OrganisationInformationContext());
    }

    private IOrganisationRepository OrganisationRepository()
    {
        return new DatabaseOrganisationRepository(postgreSql.OrganisationInformationContext());
    }
}