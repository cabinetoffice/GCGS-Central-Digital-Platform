using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class GetAuthenticationKeyUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IAuthenticationKeyRepository> _repository = new();
    private GetAuthenticationKeyUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsEmptyIfNoAuthenticationKeyIsFound()
    {
        var organisationId = Guid.NewGuid();

        var found = await UseCase.Execute(organisationId);

        found.Should().BeEmpty();
    }

    [Fact]
    public async Task ItReturnsTheFoundAuthenticationKeys()
    {
        var organisationId = Guid.NewGuid();

        var org = FakeOrganisation(organisationId, 1);

        var persistenceAuthorityKeys = new List<AuthenticationKey>
                        {
                            new AuthenticationKey { Key = "k1", Name = "key-1", OrganisationId = 1, Organisation = org },
                            new AuthenticationKey { Key = "k2", Name = "key-2", OrganisationId = 1, Organisation = org }
                        };

        _repository.Setup(r => r.GetAuthenticationKeys(organisationId)).ReturnsAsync(persistenceAuthorityKeys);

        var found = await UseCase.Execute(organisationId);
        found.Should().HaveCountGreaterThan(1);
    }

    private static OrganisationInformation.Persistence.Organisation FakeOrganisation(Guid Orgid, int id)
    {
        OrganisationInformation.Persistence.Organisation org = new()
        {
            Guid = Orgid,
            Id = 1,
            Name = "FakeOrg",
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = "Tenant 101"
            },
            ContactPoints = [new OrganisationInformation.Persistence.Organisation.ContactPoint { Email = "contact@test.org" }]
        };

        org.SupplierInfo = new OrganisationInformation.Persistence.Organisation.SupplierInformation { CompletedRegAddress = true };


        return org;
    }
}