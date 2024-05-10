using CO.CDP.OrganisationInformation.Persistence;
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
            Name = "TrentTheTenant"
        });

        createdTenant.Should().Be(new Model.Tenant
        {
            Id = _generatedGuid,
            Name = "TrentTheTenant"
        });
    }

    [Fact]
    public void ItSavesNewTenantInTheRepository()
    {
        UseCase.Execute(new RegisterTenant
        {
            Name = "TrentTheTenant"
        });

        _repository.Verify(r => r.Save(It.Is<OrganisationInformation.Persistence.Tenant>(actual =>
            actual.Guid == _generatedGuid &&
            actual.Name == "TrentTheTenant"
        )), Times.Once);
    }
}