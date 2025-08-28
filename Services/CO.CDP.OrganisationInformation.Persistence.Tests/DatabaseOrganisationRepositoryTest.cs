using CO.CDP.OrganisationInformation.Persistence.Constants;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository.OrganisationRepositoryException;
using static CO.CDP.OrganisationInformation.Persistence.Tests.EntityFactory;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseOrganisationRepositoryTest(OrganisationInformationPostgreSqlFixture postgreSql)
    : IClassFixture<OrganisationInformationPostgreSqlFixture>
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
    public async Task ItSavesTheOrganisationAndAdditionalEntitiesInASingleTransaction()
    {
        await using var context = GetDbContext();
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation(name: "Organisation name before update 1");

        await repository.SaveAsync(organisation, _ =>
        {
            organisation.Name = "Updated organisation name 1";
            repository.Save(organisation);
            return Task.CompletedTask;
        });

        var foundOrganisation = await repository.Find(organisation.Guid);

        foundOrganisation.Should().NotBeNull();
        foundOrganisation.As<Organisation>().Name.Should().Be("Updated organisation name 1");
    }

    [Fact]
    public async Task ItRevertsTheTransactionIfSavingOfAdditionalEntitiesFails()
    {
        await using var context = GetDbContext();
        var repository = OrganisationRepository();

        var organisation = GivenOrganisation(name: "Organisation name before update 2");

        var act = async () => await repository.SaveAsync(organisation, _ =>
        {
            organisation.Name = "Updated organisation name 2";
            repository.Save(organisation);
            throw new Exception("Failed in transaction");
        });

        await act.Should().ThrowAsync<Exception>("Failed in transaction");

        var foundOrganisation = await repository.Find(organisation.Guid);

        foundOrganisation.Should().BeNull();
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
        var repository = OrganisationRepository();

        var organisation1 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "TheOrganisation", tenant: GivenTenant(name: "T1"));
        var organisation2 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "TheOrganisation", tenant: GivenTenant(name: "T2"));

        repository.Save(organisation1);

        repository.Invoking(r => r.Save(organisation2))
            .Should().Throw<DuplicateOrganisationException>()
            .WithMessage($"Organisation with name `TheOrganisation` already exists.");
    }

    [Fact]
    public void ItRejectsTwoOrganisationsWithTheSameNameRegardlessOfCasing()
    {
        var repository = OrganisationRepository();

        var organisation1 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "AnotherOrganisation", tenant: GivenTenant(name: "T3"));
        var organisation2 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "ANOTHERORGANISATION", tenant: GivenTenant(name: "T4"));

        repository.Save(organisation1);

        repository.Invoking(r => r.Save(organisation2))
            .Should().Throw<DuplicateOrganisationException>()
            .WithMessage($"Organisation with name `AnotherOrganisation` already exists.");
    }

    [Fact]
    public void ItRejectsTwoOrganisationsWithTheSameNameWhenCreatingTenant()
    {
        var repository = OrganisationRepository();

        var organisation1 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "Another Test Org LTD", tenant: GivenTenant(name: "Another Test Org LTD"));
        var organisation2 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "Another Test Org LTD", tenant: GivenTenant(name: "Another Test Org LTD"));

        repository.Save(organisation1);

        repository.Invoking(r => r.Save(organisation2))
            .Should().Throw<DuplicateOrganisationException>()
            .WithMessage($"Organisation with name `Another Test Org LTD` already exists.");
    }

    [Fact]
    public void ItRejectsTwoOrganisationsWithTheSameNameWhenCreatingTenantRegardlessOfCasing()
    {
        var repository = OrganisationRepository();

        var organisation1 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "Test Org LTD", tenant: GivenTenant(name: "Test Org LTD"));
        var organisation2 =
            GivenOrganisation(guid: Guid.NewGuid(), name: "TEST ORG LTD", tenant: GivenTenant(name: "TEST ORG LTD"));

        repository.Save(organisation1);

        repository.Invoking(r => r.Save(organisation2))
            .Should().Throw<DuplicateOrganisationException>()
            .WithMessage($"Organisation with name `Test Org LTD` already exists.");
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
            .Should().Throw<DuplicateOrganisationException>()
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
            Type = OrganisationType.Organisation,
            Tenant = GivenTenant(),
            Identifiers = [new Identifier
            {
                Primary = true,
                Scheme = "GB-COH",
                IdentifierId = Guid.NewGuid().ToString(),
                LegalName = "DefaultLegalName",
                Uri = "http://default.org"
            },
                new Identifier
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
            ContactPoints = [new ContactPoint
            {
                Name = "Default Contact",
                Email = "contact@default.org",
                Telephone = "123-456-7890",
                Url = "http://contact.default.org"
            }],
            BuyerInfo = new BuyerInformation
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

        var updatedOrganisation = await repository.Find(guid);
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
            new Identifier
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
    public async Task FindIncludingTenant_WhenFound_ReturnsOrganisation()
    {
        using var repository = OrganisationRepository();

        var tenant = GivenTenant();
        var organisation = GivenOrganisation(
            name: "ABC Ltd",
            tenant: tenant
        );
        repository.Save(organisation);

        var found = await repository.FindIncludingTenant(organisation.Guid);

        found.Should().BeEquivalentTo(organisation, options => options.ComparingByMembers<Organisation>());
        found.As<Organisation>().Id.Should().BePositive();
        found.As<Organisation>().Tenant.Should().Be(tenant);
    }

    [Fact]
    public async Task FindIncludingTenantByOrgId_WhenFound_ReturnsOrganisation()
    {
        using var repository = OrganisationRepository();

        var tenant = GivenTenant();
        var organisation = GivenOrganisation(
            name: "ABCD Ltd",
            tenant: tenant
        );

        repository.Save(organisation);

        var found = await repository.FindIncludingTenantByOrgId(organisation.Id);

        found.Should().BeEquivalentTo(organisation, options => options.ComparingByMembers<Organisation>());
        found.As<Organisation>().Id.Should().BePositive();
        found.As<Organisation>().Tenant.Should().Be(tenant);
    }

    [Fact]
    public async Task IsEmailUniqueWithinOrganisation_WhenDoesNotExist_ReturnsTrue()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();
        organisation.OrganisationPersons = [];
        var personEmail = "john.doe@example.com";

        await using var context = GetDbContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.IsEmailUniqueWithinOrganisation(organisation.Guid, personEmail);

        result.Should().Be(true);
    }

    [Fact]
    public async Task IsEmailUniqueWithinOrganisation_WhenDoesExist_ReturnsFalse()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();

        var organisationPerson = new OrganisationPerson()
        {
            Organisation = organisation,
            Person = new Person
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Guid = Guid.NewGuid(),
                UserUrn = "urn:1234",
            }
        };
        organisation.OrganisationPersons.Add(organisationPerson);

        await using var context = GetDbContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.IsEmailUniqueWithinOrganisation(organisation.Guid, organisationPerson.Person.Email);

        result.Should().Be(false);
    }

    [Fact]
    public async Task ItFindsOrganisationIncludingTenantPersonsAndOrganisationPersons()
    {
        using var repository = OrganisationRepository();

        var alice = GivenPerson(firstname: "Alice");
        var bob = GivenPerson(firstname: "Bob");
        var organisation = GivenOrganisation(
            personsWithScope: [(alice, []), (bob, [])],
            tenantPersons: [alice, bob]
        );

        await using var context = GetDbContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.FindIncludingPersons(organisation.Guid);

        result.As<Organisation>().OrganisationPersons.Count.Should().Be(2);
        result.As<Organisation>().Tenant.Persons.Count.Should().Be(2);
        result.As<Organisation>().OrganisationPersons[0].Person.Guid.Should().Be(alice.Guid);
        result.As<Organisation>().OrganisationPersons[1].Person.Guid.Should().Be(bob.Guid);
        result.As<Organisation>().Tenant.Persons[0].Guid.Should().Be(alice.Guid);
        result.As<Organisation>().Tenant.Persons[1].Guid.Should().Be(bob.Guid);
    }

    [Fact]
    public async Task ItFindsOrganisationEvenIfTenantPersonsAndOrganisationPersonsAreMissing()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();

        await using var context = GetDbContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.FindIncludingPersons(organisation.Guid);

        result.As<Organisation>().OrganisationPersons.Count.Should().Be(0);
        result.As<Organisation>().Tenant.Persons.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetMouSignatures_ShouldReturnEmpty_WhenNoMatch()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();

        await using var context = GetDbContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.GetMouSignatures(organisation.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMouSignatures_ShouldReturnCorrectSignatures()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();

        await using var context = GetDbContext();
        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var mou = new Mou { Guid = Guid.NewGuid(), FilePath = "" };
        context.Mou.Add(mou);

        var person = GivenPerson();
        organisation.Persons.Add(person);

        context.SaveChanges();

        context.MouSignature.Add(new MouSignature
        {
            Id = 1,
            SignatureGuid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            CreatedById = person.Id,
            CreatedBy = person,
            Name = "Jo Bloggs",
            JobTitle = "Manager",
            MouId = mou.Id,
            Mou = mou
        });
        context.MouSignature.Add(new MouSignature
        {
            Id = 2,
            SignatureGuid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            CreatedById = person.Id,
            CreatedBy = person,
            Name = "Steve V",
            JobTitle = "Director",
            MouId = mou.Id,
            Mou = mou
        });
        context.SaveChanges();


        var result = await repository.GetMouSignatures(organisation.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().OrganisationId.Should().Be(organisation.Id);
        result.First().Mou.Should().NotBeNull();
        result.First().CreatedBy.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMouSignature_ShouldReturnEmpty_WhenNoMatch()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();

        await using var context = GetDbContext();
        await context.Organisations.AddAsync(organisation);

        var mou = new Mou { Guid = Guid.NewGuid(), FilePath = "" };
        context.Mou.Add(mou);

        var person = GivenPerson();
        organisation.Persons.Add(person);

        context.SaveChanges();

        context.MouSignature.Add(new MouSignature
        {
            Id = 3,
            SignatureGuid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            CreatedById = person.Id,
            CreatedBy = person,
            Name = "Jo Bloggs",
            JobTitle = "Manager",
            MouId = mou.Id,
            Mou = mou
        });
        context.MouSignature.Add(new MouSignature
        {
            Id = 4,
            SignatureGuid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            CreatedById = person.Id,
            CreatedBy = person,
            Name = "Steve V",
            JobTitle = "Director",
            MouId = mou.Id,
            Mou = mou
        });
        context.SaveChanges();

        var result = await repository.GetMouSignature(organisation.Id, Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMouSignature_ShouldReturnCorrectSignatures()
    {
        var mousignatureGuid1 = Guid.NewGuid();
        var mousignatureGuid2 = Guid.NewGuid();
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation();

        await using var context = GetDbContext();
        await context.Organisations.AddAsync(organisation);

        var mou = new Mou { Guid = Guid.NewGuid(), FilePath = "" };
        context.Mou.Add(mou);

        var person = GivenPerson();
        organisation.Persons.Add(person);

        context.SaveChanges();

        context.MouSignature.Add(new MouSignature
        {
            Id = 5,
            SignatureGuid = mousignatureGuid1,
            OrganisationId = organisation.Id,
            Organisation = organisation,
            CreatedById = person.Id,
            CreatedBy = person,
            Name = "Jo Bloggs",
            JobTitle = "Manager",
            MouId = mou.Id,
            Mou = mou
        });
        context.MouSignature.Add(new MouSignature
        {
            Id = 6,
            SignatureGuid = mousignatureGuid2,
            OrganisationId = organisation.Id,
            Organisation = organisation,
            CreatedById = person.Id,
            CreatedBy = person,
            Name = "Steve V",
            JobTitle = "Director",
            MouId = mou.Id,
            Mou = mou
        });
        context.SaveChanges();


        var result = await repository.GetMouSignature(organisation.Id, mousignatureGuid1);


        result.Should().NotBeNull();
        result?.OrganisationId.Should().Be(organisation.Id);
        result?.SignatureGuid.Should().Be(mousignatureGuid1);
    }

    [Fact]
    public async Task SearchByName_ShouldReturnMostSimilarResults_WhenMoreResultsFoundThanFetchBatchSize()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var organisations = new List<Organisation>
        {
            GivenOrganisation(name: "Organisation 1"),
            GivenOrganisation(name: "Organisation 2"),
            GivenOrganisation(name: "Organisation 3"),
            GivenOrganisation(name: "Organisation 4"),
            GivenOrganisation(name: "Organisation 5"),
            GivenOrganisation(name: "Organisation 6")
        };

        context.Organisations.AddRange(organisations);
        context.SaveChanges();


        var result = await repository.SearchByName("Organisation 6", null, 5, 0.5);


        result.Should().NotBeNull();
        result.Should().Contain(organisations[5]);
    }

    [Fact]
    public async Task SearchByName_ShouldNotReturnConsortium_WhenLessThanTwoOrganisationsJoined()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        var org1 = GivenOrganisation(name: "Child Org 1");
        var consortiumOrg = GivenOrganisation(name: "Consortium 1", organisationType: OrganisationType.InformalConsortium);

        context.Organisations.AddRange([org1, consortiumOrg]);

        context.SaveChanges();
        context.OrganisationParties.Add(new OrganisationParty
        {
            ParentOrganisationId = consortiumOrg.Id,
            ChildOrganisationId = org1.Id,
            OrganisationRelationship = OrganisationRelationship.Consortium
        });

        context.SaveChanges();

        var result = await repository.SearchByName("Consortium 1", null, 5, 0.5);

        result.Should().NotBeNull();
        result.Should().NotContain(consortiumOrg);
    }

    [Fact]
    public async Task SearchByName_ShouldReturnConsortium_WhenAtLeastTwoOrganisationsJoined()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        var org1 = GivenOrganisation(name: "Org 1");
        var org2 = GivenOrganisation(name: "Org 2");
        var consortiumOrg = GivenOrganisation(name: "Consort 1", organisationType: OrganisationType.InformalConsortium);

        context.Organisations.AddRange([org1, org2, consortiumOrg]);

        context.SaveChanges();
        context.OrganisationParties.AddRange([new OrganisationParty
        {
            ParentOrganisationId = consortiumOrg.Id,
            ChildOrganisationId = org1.Id,
            OrganisationRelationship = OrganisationRelationship.Consortium
        },
        new OrganisationParty
        {
            ParentOrganisationId = consortiumOrg.Id,
            ChildOrganisationId = org2.Id,
            OrganisationRelationship = OrganisationRelationship.Consortium
        }]);

        context.SaveChanges();

        var result = await repository.SearchByName("Consort 1", null, 5, 0.5);

        result.Should().NotBeNull();
        result.Should().Contain(consortiumOrg);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WhenNoOrganisationsExist_ReturnsEmptyList()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var result = await repository.SearchByNameOrPpon("Test", 10, 0, "asc");

        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WhenSearchingByName_ReturnsMatchingOrganisations()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var organisation1 = GivenOrganisation(
            name: "AOrg",
            roles: [PartyRole.Buyer],
            identifiers: [new Identifier
            {
                Primary = true,
                Scheme = "GB-PPON",
                IdentifierId = "PGWZ-1758-ABCD",
                LegalName = "AOrg",
                Uri = "http://example.org"
            }]
        );
        var organisation2 = GivenOrganisation(
            name: "BOrg",
            roles: [PartyRole.Tenderer],
            identifiers: [new Identifier
            {
                Primary = true,
                Scheme = "GB-PPON",
                IdentifierId = "PGWZ-1759-EFGH",
                LegalName = "BOrg",
                Uri = "http://example.org"
            }]
        );

        await context.Organisations.AddRangeAsync(organisation1, organisation2);
        await context.SaveChangesAsync();

        var result = await repository.SearchByNameOrPpon("AOrg", 10, 0, "asc");

        result.Results.Should().HaveCount(1);
        result.Results.First().Name.Should().Be("AOrg");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WhenSearchingByPpon_ReturnsMatchingOrganisations()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var organisation1 = GivenOrganisation(
            name: "COrg",
            roles: [PartyRole.Buyer],
            identifiers: [new Identifier
            {
                Primary = true,
                Scheme = "GB-PPON",
                IdentifierId = "PGWZ-1758-ABCD",
                LegalName = "COrg",
                Uri = "http://example.org"
            }]
        );
        var organisation2 = GivenOrganisation(
            name: "DOrg",
            roles: [PartyRole.Tenderer],
            identifiers: [new Identifier
            {
                Primary = true,
                Scheme = "GB-PPON",
                IdentifierId = "PGWZ-1761-EFGH",
                LegalName = "DOrg",
                Uri = "http://example.org"
            }]
        );

        await context.Organisations.AddRangeAsync(organisation1, organisation2);
        await context.SaveChangesAsync();

        var result = await repository.SearchByNameOrPpon("PGWZ-1758-ABCD", 10, 0, "asc");

        result.Results.Should().HaveCount(1);
        result.Results.First().Name.Should().Be("COrg");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WithPagination_ReturnsCorrectPage()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var organisations = new[]
        {
            GivenOrganisation(
                name: "Org1",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-1001-AAAA",
                    LegalName = "Org1",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "Org2",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-1002-BBBB",
                    LegalName = "Org2",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "Org3",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-1003-CCCC",
                    LegalName = "Org3",
                    Uri = "http://example.org"
                }]
            ),
        };

        await context.Organisations.AddRangeAsync(organisations);
        await context.SaveChangesAsync();

        var result = await repository.SearchByNameOrPpon("Org", 1, 1, "asc");

        result.Results.Should().HaveCount(1);
        result.Results.First().Name.Should().Be("Org2");
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WithAscendingSort_ReturnsSortedResults()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var organisations = new[]
        {
            GivenOrganisation(
                name: "EOrg",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-2001-AAAA",
                    LegalName = "EOrg",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "FOrg",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-2002-BBBB",
                    LegalName = "FOrg",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "GOrg",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-2003-CCCC",
                    LegalName = "GOrg",
                    Uri = "http://example.org"
                }]
            ),
        };

        await context.Organisations.AddRangeAsync(organisations);
        await context.SaveChangesAsync();

        var result = await repository.SearchByNameOrPpon("Org", 10, 0, "asc", 0.2);

        result.Results.Should().HaveCount(3);
        result.Results.First().Name.Should().Be("EOrg");
        result.Results.Last().Name.Should().Be("GOrg");
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WithDescendingSort_ReturnsSortedResults()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var organisations = new[]
        {
            GivenOrganisation(
                name: "HOrg",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-4001-AAAA",
                    LegalName = "HOrg",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "IOrg",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-5002-BBBB",
                    LegalName = "IOrg",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "JOrg",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-5003-CCCC",
                    LegalName = "JOrg",
                    Uri = "http://example.org"
                }]
            ),
        };

        await context.Organisations.AddRangeAsync(organisations);
        await context.SaveChangesAsync();

        var result = await repository.SearchByNameOrPpon("Org", 10, 0, "desc", 0.2);

        result.Results.Should().HaveCount(3);
        result.Results.First().Name.Should().Be("JOrg");
        result.Results.Last().Name.Should().Be("HOrg");
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WithNoOrderBySpecified_ReturnsResultsInDefaultOrder()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var organisations = new[]
        {
            GivenOrganisation(
                name: "Org5",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-9001-AAAA",
                    LegalName = "Org5",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "Org6",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-9102-BBBB",
                    LegalName = "Org6",
                    Uri = "http://example.org"
                }]
            ),
        };

        await context.Organisations.AddRangeAsync(organisations);
        await context.SaveChangesAsync();

        var result = await repository.SearchByNameOrPpon("Org", 10, 0, "rel", 0.2);

        result.Results.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WithoutGbPponIdentifier_ReturnsNoResults()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var organisation = GivenOrganisation(
            name: "Org9",
            roles: [PartyRole.Buyer, PartyRole.Tenderer],
            identifiers: [new Identifier
            {
                Primary = true,
                Scheme = "GB-COH",
                IdentifierId = "12345678",
                LegalName = "Org9",
                Uri = "http://example.org"
            }]
        );

        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        var result = await repository.SearchByNameOrPpon("Org", 10, 0, "asc");

        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WithPartialPponMatch_ReturnsNoMatchingOrganisations()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var organisation = GivenOrganisation(
            name: "KOrg",
            roles: [PartyRole.Buyer, PartyRole.Tenderer],
            identifiers: [new Identifier
            {
                Primary = true,
                Scheme = "GB-PPON",
                IdentifierId = "PGWZ-1780-ABCD",
                LegalName = "KOrg",
                Uri = "http://example.org"
            }]
        );

        await context.Organisations.AddAsync(organisation);
        await context.SaveChangesAsync();

        // Search with just part of the PPON
        var result = await repository.SearchByNameOrPpon("ABCD", 10, 0, "asc");

        result.Results.Should().HaveCount(0);
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WithHighFuzzyMatchThreshold_ReturnsNoResults()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var organisations = new[]
        {
            GivenOrganisation(
                name: "Company E",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-9001-AAAA",
                    LegalName = "Org1",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "Supplier B",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-9102-BBBB",
                    LegalName = "Org2",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "Company G",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-9102-CCCC",
                    LegalName = "Org3",
                    Uri = "http://example.org"
                }]
            ),
        };

        await context.Organisations.AddRangeAsync(organisations);
        await context.SaveChangesAsync();

        var result = await repository.SearchByNameOrPpon("Comp", 10, 0, "rel", 0.9);

        result.Results.Should().HaveCount(0);
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchByNameOrPpon_WithLowFuzzyMatchThreshold_ReturnsResults()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();
        await ClearTestData(context);

        var organisations = new[]
        {
            GivenOrganisation(
                name: "Company A",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-9001-AAAA",
                    LegalName = "Org1",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "Supplier A",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-9102-BBBB",
                    LegalName = "Org2",
                    Uri = "http://example.org"
                }]
            ),
            GivenOrganisation(
                name: "Company B",
                roles: [PartyRole.Buyer, PartyRole.Tenderer],
                identifiers: [new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = "PGWZ-9102-CCCC",
                    LegalName = "Org3",
                    Uri = "http://example.org"
                }]
            ),
        };

        await context.Organisations.AddRangeAsync(organisations);
        await context.SaveChangesAsync();

        var result = await repository.SearchByNameOrPpon("Comp", 10, 0, "rel", 0.2);

        result.Results.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    private async Task ClearTestData(OrganisationInformationContext context)
    {
        await context.Database.ExecuteSqlRawAsync("DELETE FROM identifiers");
        await context.Database.ExecuteSqlRawAsync("DELETE FROM organisations");
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPaginated_WhenNoOrganisationsExist_ReturnsEmptyList()
    {
        using var repository = OrganisationRepository();

        await using var context = GetDbContext();
        context.Organisations.RemoveRange(context.Organisations);
        await context.SaveChangesAsync();

        var result = await repository.GetPaginated(PartyRole.Buyer, PartyRole.Buyer, null, 10, 0);

        result.Item1.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaginated_WhenOrganisationsExist_ReturnsPaginatedResults()
    {
        using var repository = OrganisationRepository();

        var organisation1 = GivenOrganisation(name: "Alpha Corp", roles: [PartyRole.Buyer]);
        var organisation2 = GivenOrganisation(name: "Beta Ltd", roles: [PartyRole.Buyer]);
        var organisation3 = GivenOrganisation(name: "Gamma LLC", roles: [PartyRole.Buyer]);

        await using var context = GetDbContext();
        context.Organisations.RemoveRange(context.Organisations);
        await context.Organisations.AddRangeAsync(organisation1, organisation2, organisation3);
        await context.SaveChangesAsync();

        var result = await repository.GetPaginated(PartyRole.Buyer, PartyRole.Buyer, null, 2, 0);

        result.Item1.Should().HaveCount(2);
        result.Item1.First().Name.Should().Be("Alpha Corp");
    }

    [Fact]
    public async Task GetPaginated_WithSearchText_ReturnsMatchingResults()
    {
        using var repository = OrganisationRepository();

        var organisation1 = GivenOrganisation(name: "Acme Ltd", roles: [PartyRole.Buyer]);
        var organisation2 = GivenOrganisation(name: "Beta Solutions", roles: [PartyRole.Buyer]);

        await using var context = GetDbContext();
        context.Organisations.RemoveRange(context.Organisations);
        context.Tenants.RemoveRange(context.Tenants);
        await context.Organisations.AddRangeAsync(organisation1, organisation2);
        await context.SaveChangesAsync();

        var result = await repository.GetPaginated(PartyRole.Buyer, PartyRole.Buyer, "Acme", 10, 0);

        result.Item1.Should().HaveCount(1);
        result.Item1.First().Name.Should().Be("Acme Ltd");
    }

    [Fact]
    public async Task FindByOrganisationEmail_WhenMatchingOrganisationsExist_ShouldReturnResults()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var email = "contact@example.com";

        var organisation1 = GivenOrganisation();
        organisation1.ContactPoints = [new ContactPoint { Email = email }];

        var organisation2 = GivenOrganisation();
        organisation2.ContactPoints = [new ContactPoint { Email = email }];

        await context.Organisations.AddRangeAsync(organisation1, organisation2);
        await context.SaveChangesAsync();

        var result = await repository.FindByOrganisationEmail(email, null, null);

        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(organisation1);
        result.Should().ContainEquivalentOf(organisation2);
    }

    [Fact]
    public async Task FindByOrganisationEmail_WhenNoMatchingOrganisationsExist_ShouldReturnEmpty()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var email = "nomatch@example.com";

        var result = await repository.FindByOrganisationEmail(email, null, null);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindByOrganisationEmail_WithRoleFilter_ShouldReturnMatchingOrganisations()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var email = "buyer@example.com";
        var roleWeWant = PartyRole.Buyer;
        var roleWeDontWant = PartyRole.Supplier;

        var organisation1 = GivenOrganisation(roles: [roleWeWant]);
        organisation1.ContactPoints = [new ContactPoint { Email = email }];
        organisation1.Roles.Add(roleWeWant);

        var organisation2 = GivenOrganisation(roles: [roleWeDontWant]);
        organisation2.ContactPoints = [new ContactPoint { Email = email }];

        await context.Organisations.AddRangeAsync(organisation1, organisation2);
        await context.SaveChangesAsync();

        var result = await repository.FindByOrganisationEmail(email, roleWeWant, null);

        result.Should().HaveCount(1);
        result.Should().ContainEquivalentOf(organisation1);
        result.Should().NotContain(organisation2);
    }

    [Fact]
    public async Task FindByOrganisationEmail_WithLimit_ShouldReturnLimitedResults()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var email = "john@johnson.com";

        var organisation1 = GivenOrganisation();
        organisation1.ContactPoints = [new ContactPoint { Email = email }];

        var organisation2 = GivenOrganisation();
        organisation2.ContactPoints = [new ContactPoint { Email = email }];

        await context.Organisations.AddRangeAsync(organisation1, organisation2);
        await context.SaveChangesAsync();

        var result = await repository.FindByOrganisationEmail(email, null, 1);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task FindByAdminEmail_WhenAdminExists_ShouldReturnMatchingOrganisations()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var email = "admin@example.com";

        var person = GivenPerson(email: email);
        var organisation1 = GivenOrganisation(personsWithScope: [(person, [OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor])]);
        var organisation2 = GivenOrganisation(personsWithScope: [(person, [OrganisationPersonScopes.Editor])]);

        await context.Organisations.AddRangeAsync(organisation1, organisation2);
        await context.SaveChangesAsync();

        var result = await repository.FindByAdminEmail(email, null, null);

        result.Should().HaveCount(1);
        result.Should().ContainEquivalentOf(organisation1);
    }

    [Fact]
    public async Task FindByAdminEmail_WhenNoMatchingAdminExists_ShouldReturnEmpty()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var email = "notadmin@example.com";

        var result = await repository.FindByAdminEmail(email, null, null);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindByAdminEmail_WithRoleFilter_ShouldReturnMatchingOrganisations()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var email = "admin-role@example.com";
        var roleWeWant = PartyRole.Buyer;
        var roleWeDontWant = PartyRole.Supplier;

        var person = GivenPerson(email: email);
        var organisation1 = GivenOrganisation(roles: [roleWeWant], personsWithScope: [(person, [OrganisationPersonScopes.Admin])]);
        var organisation2 = GivenOrganisation(roles: [roleWeDontWant], personsWithScope: [(person, [OrganisationPersonScopes.Admin])]);

        await context.Organisations.AddRangeAsync(organisation1, organisation2);
        await context.SaveChangesAsync();

        var result = await repository.FindByAdminEmail(email, roleWeWant, null);

        result.Should().HaveCount(1);
        result.Should().ContainEquivalentOf(organisation1);
        result.Should().NotContain(organisation2);
    }

    [Fact]
    public async Task FindByAdminEmail_WithLimit_ShouldReturnLimitedResults()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var email = "admin-limit@example.com";

        var person = GivenPerson(email: email);
        var organisation1 = GivenOrganisation(personsWithScope: [(person, ["ADMIN"])]);
        var organisation2 = GivenOrganisation(personsWithScope: [(person, ["ADMIN"])]);

        await context.Organisations.AddRangeAsync(organisation1, organisation2);
        await context.SaveChangesAsync();

        var result = await repository.FindByAdminEmail(email, null, 1);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchByName_WithBuyerRoleAndIncludePendingRoleFalse_ReturnsOnlyOrganisationsWithBuyerRole()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var orgs = new[]
        {
            GivenOrganisation(
                name: "TestOrg1",
                roles: new List<PartyRole> { PartyRole.Buyer },
                pendingRoles: new List<PartyRole>()
            ),
            GivenOrganisation(
                name: "TestOrg2",
                roles: new List<PartyRole> { PartyRole.Buyer },
                pendingRoles: new List<PartyRole> { PartyRole.Supplier }
            ),
            GivenOrganisation(
                name: "TestOrg3",
                roles: new List<PartyRole>(),
                pendingRoles: new List<PartyRole> { PartyRole.Buyer }
            ),
            GivenOrganisation(
                name: "TestOrg4",
                roles: new List<PartyRole> { PartyRole.Supplier },
                pendingRoles: new List<PartyRole>()
            ),
            GivenOrganisation(
                name: "TestOrg5",
                roles: new List<PartyRole> { PartyRole.Buyer, PartyRole.Supplier },
                pendingRoles: new List<PartyRole> { PartyRole.Tenderer }
            )
        };

        await context.Organisations.AddRangeAsync(orgs);
        await context.SaveChangesAsync();

        var results = await repository.SearchByName("TestOrg", PartyRole.Buyer, null, 0.3, false);

        results.Should().HaveCount(3);
        results.Should().Contain(o => o.Name == "TestOrg1");
        results.Should().Contain(o => o.Name == "TestOrg2"); // Has pending roles
        results.Should().NotContain(o => o.Name == "TestOrg3"); // No active buyer role
        results.Should().NotContain(o => o.Name == "TestOrg4"); // No buyer role
        results.Should().Contain(o => o.Name == "TestOrg5"); // Has pending roles
    }

    [Fact]
    public async Task SearchByName_WithNoRoleAndIncludePendingRoleTrue_ReturnsAllOrganisations()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var orgs = new[]
        {
            GivenOrganisation(
                name: "SearchOrg1",
                roles: new List<PartyRole> { PartyRole.Buyer },
                pendingRoles: new List<PartyRole>()
            ),
            GivenOrganisation(
                name: "SearchOrg2",
                roles: new List<PartyRole>(),
                pendingRoles: new List<PartyRole> { PartyRole.Supplier }
            ),
            GivenOrganisation(
                name: "SearchOrg3",
                roles: new List<PartyRole> { PartyRole.Tenderer },
                pendingRoles: new List<PartyRole> { PartyRole.Buyer }
            ),
            GivenOrganisation(
                name: "SearchOrg4",
                roles: new List<PartyRole>(),
                pendingRoles: new List<PartyRole>()
            ),
            GivenOrganisation(
                name: "SearchOrg5",
                roles: new List<PartyRole> { PartyRole.Buyer },
                pendingRoles: new List<PartyRole> { PartyRole.Supplier }
            )
        };

        await context.Organisations.AddRangeAsync(orgs);
        await context.SaveChangesAsync();

        var results = await repository.SearchByName("SearchOrg", null, null, 0.3, true);

        results.Should().HaveCount(5);
        results.Should().Contain(o => o.Name == "SearchOrg1" && o.PendingRoles.Count == 0);
        results.Should().Contain(o => o.Name == "SearchOrg2" && o.PendingRoles.Contains(PartyRole.Supplier));
        results.Should().Contain(o => o.Name == "SearchOrg3" && o.PendingRoles.Contains(PartyRole.Buyer));
        results.Should().Contain(o => o.Name == "SearchOrg4" && o.PendingRoles.Count == 0);
        results.Should().Contain(o => o.Name == "SearchOrg5" && o.PendingRoles.Contains(PartyRole.Supplier));
    }

    [Fact]
    public async Task SearchByName_WithNoRoleAndIncludePendingRoleFalse_ReturnsOrganisationsWithoutPendingRoles()
    {
        using var repository = OrganisationRepository();
        await using var context = GetDbContext();

        var orgs = new[]
        {
            GivenOrganisation(
                name: "FilterOrg1",
                roles: new List<PartyRole> { PartyRole.Buyer },
                pendingRoles: new List<PartyRole>()
            ),
            GivenOrganisation(
                name: "FilterOrg2",
                roles: new List<PartyRole> { PartyRole.Supplier },
                pendingRoles: new List<PartyRole> { PartyRole.Buyer }
            ),
            GivenOrganisation(
                name: "FilterOrg3",
                roles: new List<PartyRole> { PartyRole.Buyer },
                pendingRoles: new List<PartyRole> { PartyRole.Supplier }
            ),
            GivenOrganisation(
                name: "FilterOrg4",
                roles: new List<PartyRole>(),
                pendingRoles: new List<PartyRole>()
            ),
            GivenOrganisation(
                name: "FilterOrg5",
                roles: new List<PartyRole> { PartyRole.Tenderer },
                pendingRoles: new List<PartyRole>()
            )
        };

        await context.Organisations.AddRangeAsync(orgs);
        await context.SaveChangesAsync();

        var results = await repository.SearchByName("FilterOrg", null, null, 0.3, false);

        results.Should().HaveCount(3);
        results.Should().Contain(o => o.Name == "FilterOrg1" && o.PendingRoles.Count == 0);
        results.Should().NotContain(o => o.Name == "FilterOrg2"); // Has pending roles
        results.Should().NotContain(o => o.Name == "FilterOrg3"); // Has pending roles
        results.Should().Contain(o => o.Name == "FilterOrg4" && o.PendingRoles.Count == 0);
        results.Should().Contain(o => o.Name == "FilterOrg5" && o.PendingRoles.Count == 0);
    }

    private DatabaseOrganisationRepository OrganisationRepository()
        => new(GetDbContext());

    private OrganisationInformationContext? context = null;

    private OrganisationInformationContext GetDbContext()
    {
        context = context ?? postgreSql.OrganisationInformationContext();

        return context;
    }
}

