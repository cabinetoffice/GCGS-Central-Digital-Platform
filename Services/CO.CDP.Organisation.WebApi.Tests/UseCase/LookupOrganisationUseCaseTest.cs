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
            Tenant = new Tenant
            {
                Id = 102,
                Guid = Guid.NewGuid(),
                Name = "Tenant 102"
            },
            Identifier = new OrganisationInformation.Persistence.Organisation.OrganisationIdentifier
            {
                Id = "Identifier1",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = "http://example.com",
                Number = "123456"
            },
            AdditionalIdentifiers = new List<OrganisationInformation.Persistence.Organisation.OrganisationIdentifier>
            {
                new OrganisationInformation.Persistence.Organisation.OrganisationIdentifier
                {
                    Id = "Identifier2",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = "http://another-example.com",
                    Number = "123456"
                }
            },
            Address = new OrganisationInformation.Persistence.Organisation.OrganisationAddress
            {
                AddressLine1 = "1234 Test St",
                City = "Test City",
                PostCode = "12345",
                Country = "Testland"
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
            Identifier = new Model.OrganisationIdentifier
            {
                Id = "Identifier1",
                Scheme = "Scheme1",
                LegalName = "Legal Name",
                Uri = "http://example.com",
                Number = "123456"
            },
            AdditionalIdentifiers = new List<Model.OrganisationIdentifier>
            {
                new Model.OrganisationIdentifier
                {
                    Id = "Identifier2",
                    Scheme = "Scheme2",
                    LegalName = "Another Legal Name",
                    Uri = "http://another-example.com",
                    Number = "123456"
                }
            },
            Address = new Model.OrganisationAddress
            {
                AddressLine1 = "1234 Test St",
                City = "Test City",
                PostCode = "12345",
                Country = "Testland"
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
