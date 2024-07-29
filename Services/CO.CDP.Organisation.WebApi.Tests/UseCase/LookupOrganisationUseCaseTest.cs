using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class LookupOrganisationUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _repository = new();
    private LookupOrganisationUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_IfNoOrganisationIsFound_ReturnsNull()
    {
        var foundRecord = await UseCase.Execute(new OrganisationQuery(name: "Test Organisation"));

        foundRecord.Should().BeNull();
    }

    [Fact]
    public async Task Execute_IfOrganisationIsFoundByName_ReturnsOrganisation()
    {
        var organisationId = Guid.NewGuid();

        var persistenceOrganisation = GivenPersistenceOrganisationInfo(organisationId);

        _repository.Setup(r => r.FindByName(persistenceOrganisation.Name)).ReturnsAsync(persistenceOrganisation);

        var foundRecord = await UseCase.Execute(new OrganisationQuery(name: "Test Organisation"));

        foundRecord.Should().BeEquivalentTo(GivenModelOrganisationInfo(organisationId), options => options.ComparingByMembers<Model.Organisation>());

    }

    [Fact]
    public async Task Execute_IfNoOrganisationIsFoundByIdentifier_ReturnsNull()
    {
        var foundRecord = await UseCase.Execute(new OrganisationQuery(identifier: "Scheme:123456"));

        foundRecord.Should().BeNull();
    }

    [Fact]
    public async Task Execute_IfOrganisationIsFoundByIdentifier_ReturnsOrganisation()
    {
        var organisationId = Guid.NewGuid();

        var persistenceOrganisation = GivenPersistenceOrganisationInfo(organisationId);

        _repository.Setup(r => r.FindByIdentifier("Scheme", "123456")).ReturnsAsync(persistenceOrganisation);

        var foundRecord = await UseCase.Execute(new OrganisationQuery(identifier: "Scheme:123456"));

        foundRecord.Should().BeEquivalentTo(GivenModelOrganisationInfo(organisationId), options => options.ComparingByMembers<Model.Organisation>());

    }

    [Fact]
    public async Task Execute_IfBothNameAndIdentifierAreMissing_ThrowsInvalidQueryException()
    {
        Func<Task> act = async () => await UseCase.Execute(new OrganisationQuery());

        await act.Should().ThrowAsync<InvalidQueryException>().WithMessage("Both name and identifier are missing from the request.");
    }

    [Fact]
    public async Task Execute_IfInvalidIdentifierFormat_ThrowsInvalidQueryException()
    {
        var query = new OrganisationQuery(identifier: "InvalidIdentifier");

        Func<Task> act = async () => await UseCase.Execute(query);

        await act.Should().ThrowAsync<InvalidQueryException>().WithMessage("Both name and identifier are missing from the request.");
    }

    [Fact]
    public async Task Execute_IfBothNameAndIdentifierAreProvided_ThrowsInvalidQueryException()
    {
        var query = new OrganisationQuery(name: "Test Organisation", identifier: "Scheme:123456");

        Func<Task> act = async () => await UseCase.Execute(query);

        await act.Should().ThrowAsync<InvalidQueryException>().WithMessage("Both name and identifier cannot be provided together.");
    }

    private OrganisationInformation.Persistence.Organisation GivenPersistenceOrganisationInfo(Guid organisationId)
    {
        return new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = organisationId,
            Name = "Test Organisation",
            Tenant = new Tenant
            {
                Id = 102,
                Guid = Guid.NewGuid(),
                Name = "Tenant 102"
            },
            Identifiers = [new OrganisationInformation.Persistence.Organisation.Identifier
            {
                Primary = true,
                IdentifierId = "123456",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = "https://example.com"
            },
                new OrganisationInformation.Persistence.Organisation.Identifier
                {
                    Primary = false,
                    IdentifierId = "123456",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = "https://another-example.com"
                }],
            Addresses = {new OrganisationInformation.Persistence.Organisation.OrganisationAddress
            {
                Type = AddressType.Registered,
                Address = new Address{
                    StreetAddress = "1234 Test St",
                    Locality = "Test City",
                    PostalCode = "12345",
                    CountryName = "Testland",
                    Region = ""
                }
            }},
            ContactPoints = [new OrganisationInformation.Persistence.Organisation.ContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                Url = "https://contact.test.org"
            }],
            Roles = [PartyRole.Tenderer]
        };

    }

    private Model.Organisation GivenModelOrganisationInfo(Guid organisationId)
    {
        return new Model.Organisation
        {
            Id = organisationId,
            Name = "Test Organisation",
            Identifier = new Identifier
            {
                Id = "123456",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = new Uri("https://example.com")
            },
            AdditionalIdentifiers =
            [
                new Identifier
                {
                    Id = "123456",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = new Uri("https://another-example.com")
                }
            ],
            Addresses = [new OrganisationInformation.Address
            {
                Type = AddressType.Registered,
                StreetAddress = "1234 Test St",
                Locality = "Test City",
                PostalCode = "12345",
                CountryName = "Testland",
                Region = ""
            }],
            ContactPoint = new ContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                Url = new Uri("https://contact.test.org")
            },
            Roles = [PartyRole.Tenderer]
        };
    }
}