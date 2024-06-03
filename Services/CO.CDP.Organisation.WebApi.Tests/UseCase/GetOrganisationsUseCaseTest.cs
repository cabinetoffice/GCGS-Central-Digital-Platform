using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class GetOrganisationsUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _repository = new();
    private GetOrganisationsUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task WhenNoOrganisationFound_ReturnEmptyList()
    {
        var userUrn = "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2GodMwe9";

        var found = await UseCase.Execute(userUrn);

        found.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenOrganisationsFound_ReturnThatList()
    {
        var userUrn = "urn:fdc:gov.uk:2022:7wTqYGMFQxgukTSpSI2GodMwe9";
        var persistenceOrganisation = new[]{ new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = Guid.NewGuid(),
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
                Scheme = "Scheme1",
                IdentifierId = "123456",
                LegalName = "Test Organisation Ltd"
            }],
            Addresses = {new OrganisationInformation.Persistence.Organisation.OrganisationAddress
            {
                Type  = AddressType.Registered,
                Address = new Address
                {
                    StreetAddress = "1234 Test St",
                    StreetAddress2 = "High Tower",
                    Locality = "Test City",
                    PostalCode = "12345",
                    CountryName = "Testland"
                }
            }},
            ContactPoint = new OrganisationInformation.Persistence.Organisation.OrganisationContactPoint
            {
                Email = "contact@test.org"
            },
            Roles = [PartyRole.Buyer]
        },
        new OrganisationInformation.Persistence.Organisation
        {
            Id = 2,
            Guid = Guid.NewGuid(),
            Name = "Test Organisation 2",
            Tenant = new Tenant
            {
                Id = 101,
                Guid = Guid.NewGuid(),
                Name = "Tenant 101"
            },
            Identifiers = [new OrganisationInformation.Persistence.Organisation.OrganisationIdentifier
            {
                Primary = true,
                Scheme = "Scheme1",
                IdentifierId = "123456",
                LegalName = "Test Organisation Ltd"
            }],
            Addresses = {new OrganisationInformation.Persistence.Organisation.OrganisationAddress
            {
                Type = AddressType.Registered,
                Address = new Address
                {
                    StreetAddress = "1234 Test St",
                    StreetAddress2 = "High Tower",
                    Locality = "Test City",
                    PostalCode = "12345",
                    CountryName = "Testland"
                }
            }},
            ContactPoint = new OrganisationInformation.Persistence.Organisation.OrganisationContactPoint
            {
                Email = "contact@test.org"
            },
            Roles = [PartyRole.Buyer]
        }};

        _repository.Setup(r => r.FindByUserUrn(userUrn)).ReturnsAsync(persistenceOrganisation);

        var found = await UseCase.Execute(userUrn);

        found.Should().HaveCount(2);
    }
}