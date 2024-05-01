using CO.CDP.Organisation.Persistence;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
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
        var persistenceOrganisation = new Persistence.Organisation
        {
            Id = 1,
            Guid = organisationId,
            Name = "Test Organisation",
            Identifier = new Persistence.Organisation.OrganisationIdentifier
            {
                Id = "Identifier1",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = "http://example.com"
            },
            AdditionalIdentifiers = new List<Persistence.Organisation.OrganisationIdentifier>
        {
            new Persistence.Organisation.OrganisationIdentifier
            {
                Id = "Identifier2",
                Scheme = "Scheme2",
                LegalName = "Another Legal Name",
                Uri = "http://another-example.com"
            }
        },
            Address = new Persistence.Organisation.OrganisationAddress
            {
                StreetAddress = "1234 Test St",
                Locality = "Test City",
                Region = "Test Region",
                PostalCode = "12345",
                CountryName = "Testland"
            },
            ContactPoint = new Persistence.Organisation.OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@test.org",
                Telephone = "123-456-7890",
                FaxNumber = "123-456-7891",
                Url = "http://contact.test.org"
            },
            Roles = new List<int> { 1 }
        };

        _repository.Setup(r => r.Find(organisationId)).ReturnsAsync(persistenceOrganisation);

        var found = await UseCase.Execute(organisationId);
        var expectedModelOrganisation = new Model.Organisation
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
        };

        found.Should().BeEquivalentTo(expectedModelOrganisation, options => options.ComparingByMembers<Model.Organisation>());
    }
}
