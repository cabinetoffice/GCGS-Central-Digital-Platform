using CO.CDP.Authentication;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class GetMyOrganisationUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _repository = new();
    private readonly Mock<IClaimService> _claimService = new();
    private GetMyOrganisationUseCase UseCase => new(_repository.Object, mapperFixture.Mapper, _claimService.Object);

    [Fact]
    public async Task ItReturnsNullIfNoOrganisationIsFound()
    {
        _claimService.Setup(r => r.GetOrganisationId()).Returns((Guid?)null);
        await UseCase.Invoking(x => x.Execute()).Should().ThrowAsync<MissingOrganisationIdException>();
    }

    [Fact]
    public async Task ItReturnsTheFoundOrganisation()
    {
        var organisationId = Random.Shared.Next(1, 100);
        var organisationGuid = Guid.NewGuid();

        var persistenceOrganisation = new OrganisationInformation.Persistence.Organisation
        {
            Id = organisationId,
            Guid = organisationGuid,
            Name = "Test Organisation",
            Tenant = new Tenant
            {
                Id = 101,
                Guid = Guid.NewGuid(),
                Name = "Tenant 101"
            },
            Identifiers = [
                new OrganisationInformation.Persistence.Organisation.Identifier
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
            Roles = [PartyRole.Buyer]
        };

        _claimService.Setup(r => r.GetOrganisationId()).Returns(organisationGuid);
        _repository.Setup(r => r.Find(organisationGuid)).ReturnsAsync(persistenceOrganisation);

        var found = await UseCase.Execute();
        var expectedModelOrganisation = new Model.Organisation
        {
            Id = organisationGuid,
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
            Roles = [PartyRole.Buyer]
        };

        found.Should().BeEquivalentTo(expectedModelOrganisation, options => options.ComparingByMembers<Model.Organisation>());
    }
}