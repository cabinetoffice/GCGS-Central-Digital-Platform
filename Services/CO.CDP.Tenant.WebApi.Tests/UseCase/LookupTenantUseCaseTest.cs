using CO.CDP.Authentication;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.Tests.AutoMapper;
using CO.CDP.Tenant.WebApi.UseCase;
using FluentAssertions;
using Moq;
using TenantLookup = CO.CDP.OrganisationInformation.Persistence.TenantLookup;

namespace CO.CDP.Tenant.WebApi.Tests.UseCase;

public class LookupTenantUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<ITenantRepository> _repository = new();
    private readonly Mock<IClaimService> _claimService = new();
    private LookupTenantUseCase UseCase => new(_repository.Object, mapperFixture.Mapper, _claimService.Object);

    [Fact]
    public async Task Execute_IfNoTenantIsFound_ReturnsNull()
    {
        var userUrn = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302";
        _claimService.Setup(cs => cs.GetUserUrn()).Returns(userUrn);
        _repository.Setup(r => r.LookupTenant(userUrn)).ReturnsAsync((TenantLookup?)null);

        var found = await UseCase.Execute();

        found.Should().BeNull();
    }

    [Fact]
    public async Task Execute_IfTenantIsFound_ReturnsTenant()
    {
        var tenantId = Guid.NewGuid();
        var userUrn = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302";
        var userTenantLookup = new TenantLookup
        {
            User = new TenantLookup.PersonUser
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
                            Roles = [PartyRole.Tenderer],
                            Scopes = ["ADMIN"]
                        }
                    ]
                }
            ]
        };

        _claimService.Setup(cs => cs.GetUserUrn()).Returns(userUrn);
        _repository.Setup(r => r.LookupTenant(userUrn)).ReturnsAsync(userTenantLookup);

        var found = await UseCase.Execute();

        found.Should().BeEquivalentTo(new OrganisationInformation.TenantLookup
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
                            Roles = [PartyRole.Tenderer],
                            Scopes = ["ADMIN"],
                            Uri = new Uri($"https://cdp.cabinetoffice.gov.uk/organisations/dfd0c5d3-0740-4be4-aa42-e42ec9c00bad")
                        }
                    ]
                }
            ]
        });
    }

    [Fact]
    public async Task Execute_IfUserUrnIsNull_ThrowsUnknownTokenException()
    {
        _claimService.Setup(cs => cs.GetUserUrn()).Returns((string?)null);

        Func<Task> act = UseCase.Execute;

        await act.Should().ThrowAsync<MissingUserUrnException>()
            .WithMessage("Ensure the token is valid and contains the necessary claims.");
    }

    [Fact]
    public async Task Execute_IfUserUrnIsEmpty_ThrowsUnknownTokenException()
    {
        _claimService.Setup(cs => cs.GetUserUrn()).Returns(string.Empty);

        Func<Task> act = UseCase.Execute;

        await act.Should().ThrowAsync<MissingUserUrnException>()
            .WithMessage("Ensure the token is valid and contains the necessary claims.");
    }

}