using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class GetOrganisationUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _repository = new();
    private GetOrganisationUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsNullIfNoOrganisationIsFound()
    {
        var organisationId = Guid.NewGuid();

        var found = await UseCase.Execute(organisationId);

        found.Should().BeNull();
    }

    [Fact]
    public async Task ItReturnsTheFoundOrganisation()
    {
        var organisationId = Guid.NewGuid();
        var persistenceOrganisation = new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = organisationId,
            Name = "Test Organisation",
            Type = OrganisationType.Organisation,
            Tenant = new Tenant
            {
                Id = 101,
                Guid = Guid.NewGuid(),
                Name = "Tenant 101"
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
                Address = new Address
                {
                    StreetAddress = "1234 Test St",
                    Locality = "Test City",
                    PostalCode = "12345",
                    CountryName = "Testland",
                    Country = "AB",
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
            Roles = [PartyRole.Buyer],
            PendingRoles = [PartyRole.InterestedParty]
        };

        _repository.Setup(r => r.Find(organisationId)).ReturnsAsync(persistenceOrganisation);

        var found = await UseCase.Execute(organisationId);
        var expectedModelOrganisation = new Model.Organisation
        {
            Id = organisationId,
            Name = "Test Organisation",
            Type = OrganisationType.Organisation,
            Identifier = new Identifier
            {
                Id = "123456",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = new Uri("https://example.com")
            },
            AdditionalIdentifiers =
            [
                new()
                {
                    Id = "123456",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = new Uri("https://another-example.com"),
                }
            ],
            Addresses = [new OrganisationInformation.Address
            {
                Type = AddressType.Registered,
                StreetAddress = "1234 Test St",
                Locality = "Test City",
                PostalCode = "12345",
                CountryName = "Testland",
                Country = "AB",
                Region = ""
            }],
            ContactPoint = new ContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                Url = new Uri("https://contact.test.org")
            },
            Roles = [PartyRole.Buyer],
            Details = new Details
            {
                PendingRoles = [PartyRole.InterestedParty]
            }
        };

        found.Should().BeEquivalentTo(expectedModelOrganisation, options => options.ComparingByMembers<Model.Organisation>());
    }
}