using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class LookupOrganisationUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _repository = new();
    private LookupOrganisationUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_IfNoOrganisationIsFound_ReturnsNull()
    {
        var name = "Test Organisation";

        var found = await UseCase.Execute(name);

        found.Should().BeNull();
    }

    [Fact]
    public async Task Execute_IfOrganisationIsFound_ReturnsOrganisation()
    {
        var organisationId = Guid.NewGuid();
        var persistenceOrganisation = new OrganisationInformation.Persistence.Organisation
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
            Address = new OrganisationInformation.Persistence.Organisation.OrganisationAddress
            {
                StreetAddress = "1234 Test St",
                StreetAddress2 = "",
                Locality = "Test City",
                PostalCode = "12345",
                CountryName = "Testland"
            },
            ContactPoint = new OrganisationInformation.Persistence.Organisation.OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                Url = "http://contact.test.org"
            },
            Types = new List<int> { 1 }
        };

        _repository.Setup(r => r.FindByName(persistenceOrganisation.Name)).ReturnsAsync(persistenceOrganisation);

        var found = await UseCase.Execute("Test Organisation");

        found.Should().BeEquivalentTo(new Model.Organisation
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
                new Identifier
                {
                    Id = "123456",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = new Uri("http://another-example.com")
                }
            ],
            Address = new Address
            {
                StreetAddress = "1234 Test St",
                StreetAddress2 = "",
                Locality = "Test City",
                PostalCode = "12345",
                CountryName = "Testland",
                Region = ""
            },
            ContactPoint = new Model.OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                Url = "http://contact.test.org"
            },
            Types = new List<int> { 1 }
        }, options => options.ComparingByMembers<Model.Organisation>());
    }
}
