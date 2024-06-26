using CO.CDP.Tenant.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Tenant.WebApi.Tests.Api;

public class GetTenantTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<Guid, Model.Tenant?>> _getTenantUseCase = new();

    public GetTenantTest()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services =>
                services.AddScoped<IUseCase<Guid, Model.Tenant?>>(_ => _getTenantUseCase.Object)
            );
        });
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ItDoesNotFindTheTenant()
    {
        var tenantId = Guid.NewGuid();

        _getTenantUseCase.Setup(useCase => useCase.Execute(tenantId))
            .Returns(Task.FromResult<Model.Tenant?>(null));

        var response = await _httpClient.GetAsync($"/tenants/{tenantId}");

        response.Should().HaveStatusCode(NotFound);
    }

    [Fact]
    public async Task ItFindsTheTenant()
    {
        var tenant = GivenTenant();

        _getTenantUseCase.Setup(useCase => useCase.Execute(tenant.Id))
            .Returns(Task.FromResult<Model.Tenant?>(tenant));

        var response = await _httpClient.GetAsync($"/tenants/{tenant.Id}");

        response.Should().HaveStatusCode(OK);
        await response.Should().HaveContent(tenant);
    }

    private static Model.Tenant GivenTenant()
    {
        return new Model.Tenant
        {
            Id = Guid.NewGuid(),
            Name = "TrentTheTenant:1"
        };
    }
}