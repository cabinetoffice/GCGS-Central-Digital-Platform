using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
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
                Uri = "http://example.org",
                Number = "123456"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName",
                    Uri = "http://example.com",
                    Number = "123456"
                }
            },
            Address = new OrganisationAddress
            {
                AddressLine1 = "1234 Example St",
                City = "Example City",
                PostCode = "12345",
                Country = "Exampleland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "http://example.org/contact"
            },
            Types = new List<int> { 1 }
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
            Types = command.Types
        };

        createdOrganisation.Should().BeEquivalentTo(expectedOrganisation, options => options.ComparingByMembers<Model.Organisation>());
    }

    [Fact]
    public void ItSavesNewOrganisationInTheRepository()
    {
        UseCase.Execute(new RegisterOrganisation
        {
            Name = "TheOrganisation",
            Identifier = new OrganisationIdentifier
            {
                Scheme = "ISO9001",
                Id = "1",
                LegalName = "OfficialOrganisationName",
                Uri = "http://example.org",
                Number = "123456"
            },
            AdditionalIdentifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Scheme = "ISO14001",
                    Id = "2",
                    LegalName = "AnotherOrganisationName",
                    Uri = "http://example.com",
                    Number = "123456"
                }
            },
            Address = new OrganisationAddress
            {
                AddressLine1 = "1234 Example St",
                City = "Example City",
                PostCode = "12345",
                Country = "Exampleland"
            },
            ContactPoint = new OrganisationContactPoint
            {
                Name = "Contact Name",
                Email = "contact@example.org",
                Telephone = "123-456-7890",
                Url = "http://example.org/contact"
            },
            Types = new List<int> { 1 }
        });

        _repository.Verify(r => r.Save(It.Is<OrganisationInformation.Persistence.Organisation>(o =>
             o.Guid == _generatedGuid &&
             o.Name == "TheOrganisation" &&
             o.Identifier == new OrganisationInformation.Persistence.Organisation.OrganisationIdentifier
             {
                 Scheme = "ISO9001",
                 Id = "1",
                 LegalName = "OfficialOrganisationName",
                 Uri = "http://example.org",
                 Number = "123456"
             } &&
             o.Address == new OrganisationInformation.Persistence.Organisation.OrganisationAddress
             {
                 AddressLine1 = "1234 Example St",
                 City = "Example City",
                 PostCode = "12345",
                 Country = "Exampleland"
             } &&
             o.ContactPoint == new OrganisationInformation.Persistence.Organisation.OrganisationContactPoint
             {
                 Name = "Contact Name",
                 Email = "contact@example.org",
                 Telephone = "123-456-7890",
                 Url = "http://example.org/contact"
             } &&
             o.Types.SequenceEqual(new List<int> { 1 })
         )), Times.Once);
    }
}