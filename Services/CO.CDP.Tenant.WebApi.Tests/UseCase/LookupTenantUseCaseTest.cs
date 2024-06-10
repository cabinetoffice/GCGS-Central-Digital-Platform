using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi.Tests.AutoMapper;
using CO.CDP.Tenant.WebApi.UseCase;
using FluentAssertions;
using Moq;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;

namespace CO.CDP.Tenant.WebApi.Tests.UseCase;

public class LookupTenantUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<ITenantRepository> _repository = new();
    private LookupTenantUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_IfNoTenantIsFound_ReturnsNull()
    {
        var name = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302";

        var found = await UseCase.Execute(name);

        found.Should().BeNull();
    }

    [Fact]
    public async Task Execute_IfTenantIsFound_ReturnsTenant()
    {
        var tenantId = Guid.NewGuid();
        var userUrn = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302";
        var persontId = Guid.NewGuid();
        var tenant = new OrganisationInformation.Persistence.Tenant
        {
            Id = 42,
            Guid = tenantId,
            Name = "TrentTheTenant",
        };
        var person = new OrganisationInformation.Persistence.Person
        {
            Id = 42,
            Guid = persontId,
            Email = "person@example.com",
            FirstName = "fn",
            LastName = "ln",
            UserUrn = userUrn,
            Tenants = { tenant }
        };

        var persistenceOrganisation = new[]{ new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Test Organisation",
            Tenant = new OrganisationInformation.Persistence.Tenant
            {
                Id = 42,
                Guid = tenantId,
                Name = "TrentTheTenant",
            },
            Identifiers = [new OrganisationInformation.Persistence.Organisation.Identifier
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
            Tenant = new OrganisationInformation.Persistence.Tenant
            {
                Id = 101,
                Guid = Guid.NewGuid(),
                Name = "Tenant 101"
            },
            Identifiers = [new OrganisationInformation.Persistence.Organisation.Identifier
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

        _repository.Setup(r => r.FindByUserUrn(userUrn)).ReturnsAsync(person);

        var found = await UseCase.Execute("urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302");

        found.Should().BeEquivalentTo(new Model.TenantLookup
        {
            User = new Model.UserDetails
            {
                Urn = userUrn,
                Name = "fn ln",
                Email = "person@example.com"
            },
            Tenants = new List<Model.UserTenant>
            {
                new Model.UserTenant
                {
                    Id = tenantId,
                    Name = "TrentTheTenant",
                    Organisations = new List<Model.UserOrganisation>()
                }
            }
        });
    }
}