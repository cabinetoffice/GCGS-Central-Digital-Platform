using System.Net.Http.Json;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.Tests.Api.WebApp;
using CO.CDP.Tenant.WebApi.UseCase;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Tenant.WebApi.Tests.Api;

public class RegisterTenantTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<RegisterTenant, Model.Tenant>> _registerTenantUseCase = new();

    public RegisterTenantTest()
    {
        TestWebApplicationFactory<Program> factory = new(services =>
        {
            services.AddScoped<IUseCase<RegisterTenant, Model.Tenant>>(_ => _registerTenantUseCase.Object);
        });
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ItRegistersNewTenant()
    {
        var command = GivenRegisterTenantCommand();
        var tenant = GivenTenant();

        _registerTenantUseCase.Setup(useCase => useCase.Execute(command))
            .Returns(Task.FromResult(tenant));

        var response = await _httpClient.PostAsJsonAsync("/tenants", command);

        response.Should().HaveStatusCode(Created, await response.Content.ReadAsStringAsync());
        response.Should().MatchLocation("^/tenants/[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$");
        await response.Should().HaveContent(tenant);
    }

    private static RegisterTenant GivenRegisterTenantCommand()
    {
        return new RegisterTenant
        {
            Name = "TrentTheTenant",
            ContactInfo = new TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        };
    }

    private static Model.Tenant GivenTenant()
    {
        return new Model.Tenant
        {
            Id = Guid.NewGuid(),
            Name = "TrentTheTenant",
            ContactInfo = new TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        };
    }
}