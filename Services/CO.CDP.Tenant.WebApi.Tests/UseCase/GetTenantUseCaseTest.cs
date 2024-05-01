using CO.CDP.Persistence.OrganisationInformation;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.Tests.AutoMapper;
using CO.CDP.Tenant.WebApi.UseCase;
using FluentAssertions;
using Moq;

namespace CO.CDP.Tenant.WebApi.Tests.UseCase;

public class GetTenantUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<ITenantRepository> _repository = new();
    private GetTenantUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task ItReturnsNullIfNoTenantIsFound()
    {
        var tenantId = Guid.NewGuid();

        var found = await UseCase.Execute(tenantId);

        found.Should().BeNull();
    }

    [Fact]
    public async Task ItReturnsTheFoundTenant()
    {
        var tenantId = Guid.NewGuid();
        var tenant = new Persistence.OrganisationInformation.Tenant
        {
            Id = 42,
            Guid = tenantId,
            Name = "TrentTheTenant",
            ContactInfo = new Persistence.OrganisationInformation.Tenant.TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        };

        _repository.Setup(r => r.Find(tenantId)).ReturnsAsync(tenant);

        var found = await UseCase.Execute(tenantId);

        found.Should().Be(new Model.Tenant
        {
            Id = tenantId,
            Name = "TrentTheTenant",
            ContactInfo = new TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        });
    }
}