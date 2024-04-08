using CO.CDP.Tenant.Persistence;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.Tests.AutoMapper;
using CO.CDP.Tenant.WebApi.UseCase;
using FluentAssertions;
using Moq;

namespace CO.CDP.Tenant.WebApi.Tests.UseCase;

public class LookupTenantUseCase(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<ITenantRepository> _repository = new();
    private GetTenantUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_IfNoTenantIsFound_ReturnsNull()
    {
        var tenantId = Guid.NewGuid();

        var found = await UseCase.Execute(tenantId);

        found.Should().BeNull();
    }

    [Fact]
    public async Task Execute_IfTenantIsFound_ReturnsTenant()
    {
        var tenantId = Guid.NewGuid();
        var tenant = new Persistence.Tenant
        {
            Id = 42,
            Guid = tenantId,
            Name = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302",
            ContactInfo = new Persistence.Tenant.TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        };

        _repository.Setup(r => r.FindByName(tenant.Name)).ReturnsAsync(tenant);

        var found = await UseCase.Execute(tenantId);

        found.Should().Be(new Model.Tenant
        {
            Id = tenantId,
            Name = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302",
            ContactInfo = new TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        });
    }
}