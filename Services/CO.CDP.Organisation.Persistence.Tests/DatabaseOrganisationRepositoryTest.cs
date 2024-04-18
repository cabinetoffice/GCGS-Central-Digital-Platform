using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.Organisation.Persistence.Organisation;

namespace CO.CDP.Organisation.Persistence.Tests;

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

        var organisation1 = GivenOrganisation(guid: Guid.NewGuid(), name: "TheOrganisation");
        var organisation2 = GivenOrganisation(guid: Guid.NewGuid(), name: "TheOrganisation");

        repository.Save(organisation1);

        repository.Invoking(r => r.Save(organisation2))
            .Should().Throw<IOrganisationRepository.OrganisationRepositoryException.DuplicateOrganisationException>()
            .WithMessage($"Organisation with name `TheOrganisation` already exists.");
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
        var initialName = "TheOrganisation";
        var updatedName = "TheOrganisationUpdated";

        //using var context = new OrganisationContext("Server=localhost;Database=cdp;Username=cdp_user;Password=cdp123;Trusted_Connection=True;");
        var context = new OrganisationContext(postgreSql.ConnectionString);
        var repository = new DatabaseOrganisationRepository(context);

        var organisation = new Organisation
        {
            Guid = guid,
            Name = initialName,
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "DefaultLegalName",
                Uri = "http://default.org"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>(),
            Address = new OrganisationAddress
            {
                StreetAddress = "1234 Default St",
                Locality = "Default City",
                Region = "Default Region",
                PostalCode = "12345",
                CountryName = "Defaultland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Default Contact",
                Email = "contact@default.org",
                Telephone = "123-456-7890",
                FaxNumber = "123-456-7891",
                Url = "http://contact.default.org"
            },
            Roles = new List<int> { 1 }
        };

        repository.Save(organisation);

        var organisationToUpdate = context.Organisations.FirstOrDefault(o => o.Guid == guid);
        if (organisationToUpdate != null)
        {
            organisationToUpdate.Name = updatedName;
            repository.Save(organisationToUpdate);
        }

        var updatedOrganisation = await context.Organisations.FindAsync(guid);
        updatedOrganisation.Should().NotBeNull();
        updatedOrganisation.Name.Should().Be(updatedName);

        //string scheme = "ISO9001";
        //string identifierId = "1";
        //string legalName = "DefaultLegalName";
        //string uri = "http://default.org";
        //string streetAddress = null;
        //string locality = null;
        //string region = null;
        //string postalCode = null;
        //string countryName = null;
        //string contactName = null;
        //string email = null;
        //string telephone = null;
        //string faxNumber = null;
        //string contactUri = null;

        //var guid = Guid.NewGuid();
        //using (var repository = OrganisationRepository())
        //{
        //    var organisation = GivenOrganisation(guid: guid, name: "TheOrganisation");
        //    repository.Save(organisation);
        //}

        //using (var repository = OrganisationRepository())
        //{
        //    var organisationToUpdate = repository.Find(guid).Result;
        //    organisationToUpdate.Name = "TheOrganisation1";
        //    repository.Save(organisationToUpdate);
        //}

        //using (var repository = OrganisationRepository())
        //{
        //    var found = await repository.Find(guid);

        //    found.Should().BeEquivalentTo(new Organisation
        //    {
        //        Id = 1,
        //        Guid = guid,
        //        Name = "TheOrganisation1",
        //        Identifier = new OrganisationIdentifier
        //        {
        //            Scheme = scheme,
        //            Id = identifierId,
        //            LegalName = legalName,
        //            Uri = uri
        //        },
        //        AdditionalIdentifiers = new List<OrganisationIdentifier>{
        //            new OrganisationIdentifier
        //            {
        //                Scheme = "ISO14001",
        //                Id = "2",
        //                LegalName = "AnotherLegalName",
        //                Uri = "http://example.com"
        //            }
        //        },
        //        Address = new OrganisationAddress
        //        {
        //            StreetAddress = streetAddress,
        //            Locality = locality,
        //            Region = region,
        //            PostalCode = postalCode,
        //            CountryName = countryName
        //        },
        //        ContactPoint = new OrganisationContactPoint
        //        {
        //            Name = contactName,
        //            Email = email,
        //            Telephone = telephone,
        //            FaxNumber = faxNumber,
        //            Url = contactUri
        //        },
        //        Roles = new List<int> { 1 }
        //    }, opts => opts.ComparingByMembers<Organisation>());
        //}
    }

    [Fact]
    public async Task FindByName_WhenFound_ReturnsOrganisation()
    {
        using var repository = OrganisationRepository();

        var organisation = GivenOrganisation(name: "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302");
        repository.Save(organisation);

        var found = await repository.FindByName(organisation.Name);

        found.Should().BeEquivalentTo(organisation, options => options.ComparingByMembers<Organisation>());
        found.Id.Should().BePositive();
    }

    [Fact]
    public async Task FindByName_WhenNotFound_ReturnsNull()
    {
        using var repository = OrganisationRepository();

        var found = await repository.FindByName("urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302");

        found.Should().BeNull();
    }

    private IOrganisationRepository OrganisationRepository()
    {
        return new DatabaseOrganisationRepository(OrganisationContext());
    }

    private OrganisationContext OrganisationContext()
    {
        var context = new OrganisationContext(postgreSql.ConnectionString);
        context.Database.Migrate();
        context.SaveChanges();
        return context;
    }

    private static Organisation GivenOrganisation(
    Guid? guid = null,
    string? name = null,
    string scheme = "ISO9001",
    string identifierId = "1",
    string legalName = "DefaultLegalName",
    string uri = "http://default.org",
    string streetAddress = "1234 Default St",
    string locality = "Default City",
    string region = "Default Region",
    string postalCode = "12345",
    string countryName = "Defaultland",
    string contactName = "Default Contact",
    string email = "contact@default.org",
    string telephone = "123-456-7890",
    string faxNumber = "123-456-7891",
    string contactUri = "http://contact.default.org",
    List<int>? roles = null)
    {
        var theGuid = guid ?? Guid.NewGuid();
        var theName = name ?? $"Organisation {theGuid}";

        return new Organisation
        {
            Guid = theGuid,
            Name = theName,
            Identifier = new OrganisationIdentifier
            {
                Scheme = scheme,
                Id = identifierId,
                LegalName = legalName,
                Uri = uri
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
        {
            new OrganisationIdentifier
            {
                Scheme = "ISO14001",
                Id = "2",
                LegalName = "AnotherLegalName",
                Uri = "http://example.com"
            }
        },
            Address = new OrganisationAddress
            {
                StreetAddress = streetAddress,
                Locality = locality,
                Region = region,
                PostalCode = postalCode,
                CountryName = countryName
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = contactName,
                Email = email,
                Telephone = telephone,
                FaxNumber = faxNumber,
                Url = contactUri
            },
            Roles = roles ?? new List<int> { 1 }
        };
    }
}