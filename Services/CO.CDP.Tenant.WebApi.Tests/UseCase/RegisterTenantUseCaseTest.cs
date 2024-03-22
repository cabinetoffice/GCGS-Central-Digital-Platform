using CO.CDP.Tenant.Persistence;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.Tests.AutoMapper;
using CO.CDP.Tenant.WebApi.UseCase;
using FluentAssertions;
using Moq;

namespace CO.CDP.Tenant.WebApi.Tests.UseCase;

public class RegisterTenantUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<ITenantRepository> _repository = new();
    private readonly Guid _generatedGuid = Guid.NewGuid();
    private RegisterTenantUseCase UseCase => new(_repository.Object, mapperFixture.Mapper, () => _generatedGuid);

    [Fact]
    public async Task ItReturnsTheRegisteredTenant()
    {
        var createdTenant = await UseCase.Execute(new RegisterTenant
        {
            Name = "TrentTheTenant",
            ContactInfo = new TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        });

        createdTenant.Should().Be(new Model.Tenant
        {
            Id = _generatedGuid,
            Name = "TrentTheTenant",
            ContactInfo = new TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        });
    }

    [Fact]
    public void ItSavesNewTenantInTheRepository()
    {
        UseCase.Execute(new RegisterTenant
        {
            Name = "TrentTheTenant",
            ContactInfo = new TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        });

        _repository.Verify(r => r.Save(It.Is<Persistence.Tenant>(actual =>
            actual.Guid == _generatedGuid &&
            actual.Name == "TrentTheTenant" &&
            actual.ContactInfo == new Persistence.Tenant.TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        )), Times.Once);
    }
}