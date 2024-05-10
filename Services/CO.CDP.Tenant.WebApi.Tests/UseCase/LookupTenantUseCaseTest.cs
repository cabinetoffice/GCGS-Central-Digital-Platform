using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Tenant.WebApi.Tests.AutoMapper;
using CO.CDP.Tenant.WebApi.UseCase;
using FluentAssertions;
using Moq;

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
        var tenant = new OrganisationInformation.Persistence.Tenant
        {
            Id = 42,
            Guid = tenantId,
            Name = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302"
        };

        _repository.Setup(r => r.FindByName(tenant.Name)).ReturnsAsync(tenant);

        var found = await UseCase.Execute("urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302");

        found.Should().Be(new Model.Tenant
        {
            Id = tenantId,
            Name = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302"
        });
    }
}