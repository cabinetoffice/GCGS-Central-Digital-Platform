using CO.CDP.Organisation.Persistence;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RegisterOrganisationUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _repository = new();
    private readonly Guid _generatedGuid = Guid.NewGuid();
    private RegisterOrganisationUseCase UseCase => new(_repository.Object, mapperFixture.Mapper, () => _generatedGuid);

    [Fact]
    public async Task ItReturnsTheRegisteredOrganisation()
    {
        var command = new RegisterOrganisation
        {
            Name = "TheOrganisation",
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "OfficialOrganisationName",
                Uri = "http://example.org"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName",
                    Uri = "http://example.com"
                }
            },
            Address = new OrganisationAddress
            {
                StreetAddress = "1234 Example St",
                Locality = "Example City",
                Region = "Example Region",
                PostalCode = "12345",
                CountryName = "Exampleland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                FaxNumber = "123-456-7891",
                Url = "http://example.org/contact"
            },
            Roles = new List<int> { 1 }
        };

        var createdOrganisation = await UseCase.Execute(command);

        var expectedOrganisation = new Model.Organisation
        {
            Id = _generatedGuid,
            Name = "TheOrganisation",
            Identifier = command.Identifier,
            AdditionalIdentifiers = command.AdditionalIdentifiers,
            Address = command.Address,
            ContactPoint = command.ContactPoint,
            Roles = command.Roles
        };

        createdOrganisation.Should().BeEquivalentTo(expectedOrganisation, options => options.ComparingByMembers<Model.Organisation>());
    }

    [Fact]
    public void ItSavesNewTenantInTheRepository()
    {
        UseCase.Execute(new RegisterOrganisation
        {
            Name = "TheOrganisation",
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "OfficialOrganisationName",
                Uri = "http://example.org"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName",
                    Uri = "http://example.com"
                }
            },
            Address = new OrganisationAddress
            {
                StreetAddress = "1234 Example St",
                Locality = "Example City",
                Region = "Example Region",
                PostalCode = "12345",
                CountryName = "Exampleland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                FaxNumber = "123-456-7891",
                Url = "http://example.org/contact"
            },
            Roles = new List<int> { 1 }
        });

        _repository.Verify(r => r.Save(It.Is<Persistence.Organisation>(o =>
             o.Guid == _generatedGuid &&
             o.Name == "TheOrganisation" &&
             o.Identifier == new Persistence.Organisation.OrganisationIdentifier
             {
                 Scheme = "ISO9001",
                 Id = "1",
                 LegalName = "OfficialOrganisationName",
                 Uri = "http://example.org"
             } &&
             o.Address == new Persistence.Organisation.OrganisationAddress
             {
                 StreetAddress = "1234 Example St",
                 Locality = "Example City",
                 Region = "Example Region",
                 PostalCode = "12345",
                 CountryName = "Exampleland"
             } &&
             o.ContactPoint == new Persistence.Organisation.OrganisationContactPoint
             {
                 Name = "Contact Name",
                 Email = "contact@example.org",
                 Telephone = "123-456-7890",
                 FaxNumber = "123-456-7891",
                 Url = "http://example.org/contact"
             } &&
             o.Roles.SequenceEqual(new List<int> { 1 })
         )), Times.Once);
    }
}