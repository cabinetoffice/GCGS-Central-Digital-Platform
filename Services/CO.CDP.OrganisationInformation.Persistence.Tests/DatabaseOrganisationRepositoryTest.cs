using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using static CO.CDP.OrganisationInformation.Persistence.Organisation;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseOrganisationRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ItFindsSavedOrganisation()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation(guid: Guid.NewGuid());

        repository.Save(organisation);

        var found = await repository.Find(organisation.Guid);

        found.Should().Be(organisation);
        found.As<Organisation>().Id.Should().BePositive();

    }

    [Fact]
    public async Task ItReturnsNullIfOrganisationIsNotFound()
    {
        using var repository = OrganisationRepository();

        var found = await repository.Find(Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public void ItRejectsTwoOrganisationsWithTheSameName()
    {
        using var repository = OrganisationRepository();

        var organisation1 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "TheOrganisation", tenant: GivenTenant(name: "T1"));
        var organisation2 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "TheOrganisation", tenant: GivenTenant(name: "T2"));

        repository.Save(organisation1);

        repository.Invoking(r => r.Save(organisation2))
            .Should().Throw<IOrganisationRepository.OrganisationRepositoryException.DuplicateOrganisationException>()
            .WithMessage($"Organisation with name `TheOrganisation` already exists.");
    }

    [Fact]
    public void ItRejectsTwoOrganisationsWithTheSameNameWhenCreatingTenant()
    {
        using var repository = OrganisationRepository();

        var organisation1 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "Acme LTD", tenant: GivenTenant(name: "Acme LTD"));
        var organisation2 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "Acme LTD", tenant: GivenTenant(name: "Acme LTD"));

        repository.Save(organisation1);

        repository.Invoking(r => r.Save(organisation2))
            .Should().Throw<IOrganisationRepository.OrganisationRepositoryException.DuplicateOrganisationException>()
            .WithMessage($"Organisation with name `Acme LTD` already exists.");
    }

    [Fact]
    public void ItRejectsTwoOrganisationsWithTheSameGuid()
    {
        using var repository = OrganisationRepository();

        var guid = Guid.NewGuid();
        var organisation1 = GivenOrganisation(guid: guid, name: "Organisation1");
        var organisation2 = GivenOrganisation(guid: guid, name: "Organisation2");

        repository.Save(organisation1);

        repository.Invoking((r) => r.Save(organisation2))
            .Should().Throw<IOrganisationRepository.OrganisationRepositoryException.DuplicateOrganisationException>()
            .WithMessage($"Organisation with guid `{guid}` already exists.");
    }

    [Fact]
    public async Task ItUpdatesAnExistingOrganisation()
    {
        var guid = Guid.NewGuid();
        var initialName = "TheOrganisation1";
        var updatedName = "TheOrganisationUpdated1";
        var initialDate = DateTime.UtcNow.AddDays(-1);

        var repository = OrganisationRepository();

        var organisation = new Organisation
        {
            Guid = guid,
            Name = initialName,
            Tenant = GivenTenant(),
            Identifiers = [new OrganisationIdentifier
            {
                Primary = true,
                Scheme = "ISO9001",
                IdentifierId = "1",
                LegalName = "DefaultLegalName",
                Uri = "http://default.org"
            },
                new OrganisationIdentifier
                {
                    Primary = false,
                    Scheme = "ISO9001",
                    IdentifierId = "1",
                    LegalName = "DefaultLegalName",
                    Uri = "http://default.org"
                }],
            Addresses =  {new OrganisationAddress
            {
                Type = AddressType.Registered,
                Address = new Address{
                    StreetAddress = "1234 Default St",
                    StreetAddress2 = "High Tower",
                    Locality = "London",
                    PostalCode = "12345",
                    CountryName = "Defaultland"
                }
            }},
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Default Contact",
                Email = "contact@default.org",
                Telephone = "123-456-7890",
                Url = "http://contact.default.org"
            },
            UpdatedOn = initialDate,
            CreatedOn = initialDate,
            Roles = { PartyRole.Buyer }
        };

        repository.Save(organisation);

        var organisationToUpdate = await repository.Find(guid);
        if (organisationToUpdate != null)
        {
            organisationToUpdate.Name = updatedName;
            repository.Save(organisationToUpdate);
        }


        repository.Save(organisation);

        var updatedOrganisation = await repository.Find(guid)!;
        updatedOrganisation.Should().NotBeNull();
        updatedOrganisation.As<Organisation>().Name.Should().Be(updatedName);
        updatedOrganisation.As<Organisation>().Tenant.Should().Be(organisation.Tenant);
        //updatedOrganisation.As<Organisation>().UpdatedOn.Should().BeAfter(initialDate);
    }

    [Fact]
    public async Task FindByName_WhenFound_ReturnsOrganisation()
    {
        using var repository = OrganisationRepository();

        var tenant = GivenTenant();
        var organisation = GivenOrganisation(
            name: "Acme Ltd",
            tenant: tenant
        );
        repository.Save(organisation);

        var found = await repository.FindByName(organisation.Name);

        found.Should().BeEquivalentTo(organisation, options => options.ComparingByMembers<Organisation>());
        found.As<Organisation>().Id.Should().BePositive();
        found.As<Organisation>().Tenant.Should().Be(tenant);
    }

    [Fact]
    public async Task FindByName_WhenNotFound_ReturnsNull()
    {
        using var repository = OrganisationRepository();

        var found = await repository.FindByName("urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894301");

        found.Should().BeNull();
    }

    [Fact]
    public async Task FindByUserUrn_WhenNoOrganisationFound_ReturnEmptyList()
    {
        using var repository = OrganisationRepository();
        var userUrn = "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2G0dMwe9";

        var found = await repository.FindByUserUrn(userUrn);

        found.Should().BeEmpty();
    }

    [Fact]
    public async Task FindByUserUrn_WhenOrganisationsFound_ReturnThatList()
    {
        using var repository = OrganisationRepository();
        var userUrn = "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2GodMwe9";

        var person = GivenPerson(userUrn: userUrn);

        var organisation1 = GivenOrganisation();
        organisation1.Persons.Add(person);
        repository.Save(organisation1);

        var organisation2 = GivenOrganisation();
        organisation2.Persons.Add(person);
        repository.Save(organisation2);

        var found = await repository.FindByUserUrn(userUrn);

        found.Should().HaveCount(2);
        found.Should().ContainEquivalentOf(organisation1);
    }

    private IOrganisationRepository OrganisationRepository()
    {
        return new DatabaseOrganisationRepository(postgreSql.OrganisationInformationContext());
    }
}