using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
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
            Identifier = new OrganisationInformation.Persistence.Organisation.OrganisationIdentifier
            {
                Id = "Identifier1",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = "http://example.com"
            },
            AdditionalIdentifiers = new List<OrganisationInformation.Persistence.Organisation.OrganisationIdentifier>
            {
                new OrganisationInformation.Persistence.Organisation.OrganisationIdentifier
                {
                    Id = "Identifier2",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = "http://another-example.com"
                }
            },
            Address = new OrganisationInformation.Persistence.Organisation.OrganisationAddress
            {
                StreetAddress = "1234 Test St",
                Locality = "Test City",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Testland"
            },
            ContactPoint = new OrganisationInformation.Persistence.Organisation.OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                FaxNumber = "123-456-7891",
                Url = "http://contact.test.org"
            },
            Roles = new List<int> { 1 }
        };

        _repository.Setup(r => r.FindByName(persistenceOrganisation.Name)).ReturnsAsync(persistenceOrganisation);

        var found = await UseCase.Execute("Test Organisation");

        found.Should().BeEquivalentTo(new Model.Organisation
        {
            Id = organisationId,
            Name = "Test Organisation",
            Identifier = new OrganisationIdentifier
            {
                Id = "Identifier1",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = "http://example.com"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Id = "Identifier2",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = "http://another-example.com"
                }
            },
            Address = new OrganisationAddress
            {
                StreetAddress = "1234 Test St",
                Locality = "Test City",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Testland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                FaxNumber = "123-456-7891",
                Url = "http://contact.test.org"
            },
            Roles = new List<int> { 1 }
        }, options => options.ComparingByMembers<Model.Organisation>());
    }
}
