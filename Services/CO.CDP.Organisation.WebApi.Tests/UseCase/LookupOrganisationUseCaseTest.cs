using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Persistence.OrganisationInformation;
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
        var persistenceOrganisation = new CDP.Persistence.OrganisationInformation.Organisation
        {
            Id = 1,
            Guid = organisationId,
            Name = "Test Organisation",
            Identifier = new CDP.Persistence.OrganisationInformation.Organisation.OrganisationIdentifier
            {
                Id = "Identifier1",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = "http://example.com"
            },
            AdditionalIdentifiers = new List<CDP.Persistence.OrganisationInformation.Organisation.OrganisationIdentifier>
            {
                new CDP.Persistence.OrganisationInformation.Organisation.OrganisationIdentifier
                {
                    Id = "Identifier2",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = "http://another-example.com"
                }
            },
            Address = new CDP.Persistence.OrganisationInformation.Organisation.OrganisationAddress
            {
                StreetAddress = "1234 Test St",
                Locality = "Test City",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Testland"
            },
            ContactPoint = new CDP.Persistence.OrganisationInformation.Organisation.OrganisationContactPoint
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
            Identifier = new Model.OrganisationIdentifier
            {
                Id = "Identifier1",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = "http://example.com"
            },
            AdditionalIdentifiers = new List<Model.OrganisationIdentifier>
            {
                new Model.OrganisationIdentifier
                {
                    Id = "Identifier2",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = "http://another-example.com"
                }
            },
            Address = new Model.OrganisationAddress
            {
                StreetAddress = "1234 Test St",
                Locality = "Test City",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Testland"
            },
            ContactPoint = new Model.OrganisationContactPoint
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
