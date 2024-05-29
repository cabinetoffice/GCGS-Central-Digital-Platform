using CO.CDP.Common.Enums;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

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
            Tenant = new Tenant
            {
                Id = 101,
                Guid = Guid.NewGuid(),
                Name = "Tenant 101"
            },
            Identifiers = [new OrganisationInformation.Persistence.Organisation.OrganisationIdentifier
                {
                    Primary = true,
                    IdentifierId = "123456",
                    Scheme = "Scheme1",
                    LegalName = "Legal Name",
                    Uri = "http://example.com"
                },
                new OrganisationInformation.Persistence.Organisation.OrganisationIdentifier
                {
                    Primary = false,
                    IdentifierId = "123456",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = "http://another-example.com"
                }],
            Addresses = {new OrganisationInformation.Persistence.Organisation.OrganisationAddress
            {
                Type = AddressType.Regsitered,
                Address = new OrganisationInformation.Persistence.Address
                {
                    StreetAddress = "1234 Test St",
                    StreetAddress2 = "Green Tower",
                    Locality = "Test City",
                    PostalCode = "12345",
                    CountryName = "Testland",
                    Region = ""
                }
            }},
            ContactPoint = new OrganisationInformation.Persistence.Organisation.OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                Url = "http://contact.test.org"
            },
            Types = [OrganisationType.ProcuringEntity]
        };

        _repository.Setup(r => r.Find(organisationId)).ReturnsAsync(persistenceOrganisation);

        var found = await UseCase.Execute(organisationId);
        var expectedModelOrganisation = new Model.Organisation
        {
            Id = organisationId,
            Name = "Test Organisation",
            Identifier = new Identifier
            {
                Id = "123456",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = new Uri("http://example.com")
            },
            AdditionalIdentifiers =
            [
                new()
                {
                    Id = "123456",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = new Uri("http://another-example.com"),
                }
            ],
            Address = new OrganisationInformation.Address
            {
                StreetAddress = "1234 Test St",
                StreetAddress2 = "Green Tower",
                Locality = "Test City",
                PostalCode = "12345",
                CountryName = "Testland",
                Region = ""
            },
            ContactPoint = new ContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                Url = new Uri("http://contact.test.org")
            },
            Types = new List<int> { 1 }
        };

        found.Should().BeEquivalentTo(expectedModelOrganisation, options => options.ComparingByMembers<Model.Organisation>());
    }
}
