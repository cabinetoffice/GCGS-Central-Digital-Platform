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

        var person = GivenPerson();
        var organisation = GivenOrganisation(guid: Guid.NewGuid(), personsWithScope: [(person, ["ADMIN"])]);

        repository.Save(organisation);

        var found = await repository.Find(organisation.Guid);

        found.Should().Be(organisation);
        found.As<Organisation>().Id.Should().BePositive();
        found.As<Organisation>().OrganisationPersons.First().Scopes.Should().Equal(["ADMIN"]);
    }

    [Fact]
    public async Task ItReturnsNullIfOrganisationIsNotFound()
    {
        using var repository = OrganisationRepository();

        var found = await repository.Find(Guid.NewGuid());

        found.Should().BeNull();
    }

    [Fact]
    public async Task ItFindsSavedOrganisationById()
    {
        using var repository = OrganisationRepository();

        var person = GivenPerson();
        var organisation = GivenOrganisation(personsWithScope: [(person, ["ADMIN"])]);

        repository.Save(organisation);

        var found = await repository.Find(organisation.Guid);

        found.Should().Be(organisation);
        found.As<Organisation>().Id.Should().BePositive();
        found.As<Organisation>().OrganisationPersons.First().Scopes.Should().Equal(["ADMIN"]);
    }

    [Fact]
    public async Task ItReturnsNullIfOrganisationIsNotFoundById()
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
            Identifiers = [new Organisation.Identifier
            {
                Primary = true,
                Scheme = "GB-COH",
                IdentifierId = Guid.NewGuid().ToString(),
                LegalName = "DefaultLegalName",
                Uri = "http://default.org"
            },
                new Organisation.Identifier
                {
                    Primary = false,
                    Scheme = "GB-PPON",
                    IdentifierId = Guid.NewGuid().ToString(),
                    LegalName = "DefaultLegalName",
                    Uri = "http://default.org"
                }],
            Addresses =  {new OrganisationAddress
            {
                Type = AddressType.Registered,
                Address = new Address{
                    StreetAddress = "1234 Default St",
                    Locality = "London",
                    PostalCode = "12345",
                    CountryName = "Defaultland",
                    Country = "AB"
                }
            }},
            ContactPoints = [new Organisation.ContactPoint
            {
                Name = "Default Contact",
                Email = "contact@default.org",
                Telephone = "123-456-7890",
                Url = "http://contact.default.org"
            }],
            BuyerInfo = new Organisation.BuyerInformation
            {
                BuyerType = "Buyer Type 1",
                DevolvedRegulations = [DevolvedRegulation.NorthernIreland],
                UpdatedOn = initialDate,
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
        updatedOrganisation.As<Organisation>().UpdatedOn.Should().BeAfter(initialDate);
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
    public async Task FindByIdentifier_WhenFound_ReturnsOrganisation()
    {
        using var repository = OrganisationRepository();

        var organisationId = Guid.NewGuid();
        var organisation = GivenOrganisation(
            guid: organisationId,
            identifiers:
            [
            new Organisation.Identifier
            {
                Primary = true,
                Scheme = "Scheme",
                IdentifierId = "123456",
                LegalName = "Acme Ltd",
                Uri = "https://example.com"
            }
            ]
        );
        repository.Save(organisation);

        var found = await repository.FindByIdentifier("Scheme", "123456");

        found.Should().BeEquivalentTo(organisation, options => options.ComparingByMembers<Organisation>());
        found.As<Organisation>().Id.Should().BePositive();
        found.As<Organisation>().Tenant.Should().Be(organisation.Tenant);
    }

    [Fact]
    public async Task FindByIdentifier_WhenNotFound_ReturnsNull()
    {
        using var repository = OrganisationRepository();

        var found = await repository.FindByIdentifier("NonExistentScheme", "NonExistentId");

        found.Should().BeNull();
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

        var organisation1 = GivenOrganisation(personsWithScope: [(person, ["ADMIN"])]);
        repository.Save(organisation1);

        var organisation2 = GivenOrganisation(personsWithScope: [(person, ["ADMIN"])]);
        repository.Save(organisation2);

        var found = await repository.FindByUserUrn(userUrn);

        found.As<IEnumerable<Organisation>>().Should().HaveCount(2);
        found.As<IEnumerable<Organisation>>().Should().ContainEquivalentOf(organisation1);
    }

    [Fact]
    public async Task GetConnectedIndividualTrusts_WhenConnectedEntitiesExist_ReturnsConnectedEntities()
    {
        using var repository = OrganisationRepository();

        var supplierOrganisation = GivenOrganisation();
        var connectedEntity = GivenConnectedIndividualTrust(supplierOrganisation);

        using var context = postgreSql.OrganisationInformationContext();
        await context.Organisations.AddAsync(supplierOrganisation);
        await context.ConnectedEntities.AddAsync(connectedEntity);
        await context.SaveChangesAsync();

        var result = await repository.GetConnectedIndividualTrusts(supplierOrganisation.Id);

        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);

        var individualTrust = result.First().IndividualOrTrust;
        individualTrust.Should().BeEquivalentTo(connectedEntity.IndividualOrTrust, options =>
            options
                .Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(100)))
                .WhenTypeIs<DateTimeOffset>()
        );
    }

    [Fact]
    public async Task GetConnectedIndividualTrusts_WhenNoConnectedEntitiesExist_ReturnsEmptyList()
    {
        using var repository = OrganisationRepository();

        var organisationId = 1;
        var organisation = GivenOrganisation();

        using var context = postgreSql.OrganisationInformationContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.GetConnectedIndividualTrusts(organisationId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetConnectedOrganisations_WhenConnectedEntitiesExist_ReturnsConnectedEntities()
    {
        using var repository = OrganisationRepository();

        var supplierOrganisation = GivenOrganisation();
        var connectedEntity = GivenConnectedOrganisation(supplierOrganisation);

        using var context = postgreSql.OrganisationInformationContext();
        await context.Organisations.AddAsync(supplierOrganisation);
        await context.ConnectedEntities.AddAsync(connectedEntity);
        await context.SaveChangesAsync();

        var result = await repository.GetConnectedOrganisations(supplierOrganisation.Id);

        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);

        var organisation = result.First().Organisation;
        organisation.Should().BeEquivalentTo(connectedEntity.Organisation, options =>
            options
                .Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(100)))
                .WhenTypeIs<DateTimeOffset>()
        );
    }

    [Fact]
    public async Task GetConnectedOrganisations_WhenNoConnectedEntitiesExist_ReturnsEmptyList()
    {
        using var repository = OrganisationRepository();

        var organisationId = 1;
        var organisation = GivenOrganisation();

        using var context = postgreSql.OrganisationInformationContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.GetConnectedOrganisations(organisationId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLegalForm_WhenNoLegalFormExists_ReturnNull()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();
        organisation.SupplierInfo = GivenSupplierInformation();
        organisation.SupplierInfo.LegalForm = null;

        using var context = postgreSql.OrganisationInformationContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.GetLegalForm(organisation.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLegalForm_WhenLegalFormExists_ReturnValidLegalForm()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();
        organisation.SupplierInfo = GivenSupplierInformation();
        organisation.SupplierInfo.LegalForm = GivenSupplierLegalForm();

        using var context = postgreSql.OrganisationInformationContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.GetLegalForm(organisation.Id);

        result?.Should().NotBeNull();
        result?.RegisteredUnderAct2006.Should().Be(true);
        result?.RegisteredLegalForm.Should().Be("Limited company");
        result?.LawRegistered.Should().Be("England and Wales");
        result?.RegistrationDate.Should().Be(DateTimeOffset.Parse("2005-12-02T00:00:00Z"));
    }

    [Fact]
    public async Task GetOperationTypes_WhenNoOperationTypeExists_ReturnsNull()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();
        organisation.SupplierInfo = GivenSupplierInformation();
        organisation.SupplierInfo.OperationTypes = [];

        using var context = postgreSql.OrganisationInformationContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.GetOperationTypes(organisation.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOperationTypes_WhenOperationTypeExists_ReturnsEmptyList()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();
        organisation.SupplierInfo = GivenSupplierInformation();
        organisation.SupplierInfo.OperationTypes = [OperationType.SmallOrMediumSized];

        using var context = postgreSql.OrganisationInformationContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.GetOperationTypes(organisation.Id);

        result.Should().NotBeNull();
        result.Should().Contain(OperationType.SmallOrMediumSized);
    }

    private IOrganisationRepository OrganisationRepository()
    {
        return new DatabaseOrganisationRepository(postgreSql.OrganisationInformationContext());
    }
}