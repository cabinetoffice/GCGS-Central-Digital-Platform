using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.Tests.AutoMapper;
using CO.CDP.Tenant.WebApi.UseCase;
using FluentAssertions;
using Moq;
using Address = CO.CDP.OrganisationInformation.Persistence.Address;

namespace CO.CDP.Tenant.WebApi.Tests.UseCase;

public class LookupTenantUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IPersonRepository> _repository = new();
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
        var userTenantLookup = new UserTenantLookup
        {
            User = new UserTenantLookup.PersonUser
            {
                Urn = userUrn,
                Name = "fn ln",
                Email = "person@example.com"
            },
            Tenants =
            [
                new()
                {
                    Id = tenantId,
                    Name = "TrentTheTenant",
                    Organisations =
                    [
                        new()
                        {
                            Id = Guid.Parse("dfd0c5d3-0740-4be4-aa42-e42ec9c00bad"),
                            Name = "Acme Ltd",
                            Roles = [PartyRole.Supplier],
                            Scopes = ["ADMIN"]
                        }
                    ]
                }
            ]
        };

        _repository.Setup(r => r.FindByUserUrn(userUrn)).ReturnsAsync(userTenantLookup);

        var found = await UseCase.Execute("urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302");

        found.Should().BeEquivalentTo(new TenantLookup
        {
            User = new UserDetails
            {
                Urn = userUrn,
                Name = "fn ln",
                Email = "person@example.com"
            },
            Tenants =
            [
                new UserTenant
                {
                    Id = tenantId,
                    Name = "TrentTheTenant",
                    Organisations =
                    [
                        new UserOrganisation
                        {
                            Id = Guid.Parse("dfd0c5d3-0740-4be4-aa42-e42ec9c00bad"),
                            Name = "Acme Ltd",
                            Roles = [PartyRole.Supplier],
                            Scopes = ["ADMIN"],
                            Uri = new Uri("/organisations/dfd0c5d3-0740-4be4-aa42-e42ec9c00bad")
                        }
                    ]
                }
            ]
        });
    }
}