using CO.CDP.Tenant.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Tenant.WebApi.Tests.Api;

public class TenantLookupTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<string, Model.Tenant?>> _getTenantUseCase = new();

    public TenantLookupTest()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services =>
                services.AddScoped(_ => _getTenantUseCase.Object)
            );
        });
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task IfNoTenantIsFound_ReturnsNotFound()
    {
        var urn = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302";

        _getTenantUseCase.Setup(useCase => useCase.Execute(urn))
            .Returns(Task.FromResult<Model.Tenant?>(null));

        var response = await _httpClient.GetAsync($"/tenant/lookup?urn={urn}");

        response.Should().HaveStatusCode(NotFound);
    }

    [Fact(Skip = "Tenant lookup response is being reworked.")]
    public async Task IfTenantIsFound_ReturnsTenant()
    {
        var tenant = GivenTenant();

        _getTenantUseCase.Setup(useCase => useCase.Execute(tenant.Name))
            .Returns(Task.FromResult<Model.Tenant?>(tenant));

        var response = await _httpClient.GetAsync($"/tenant/lookup?urn={tenant.Name}");

        response.Should().HaveStatusCode(OK);
        await response.Should().HaveContent(tenant);
    }

    private static Model.Tenant GivenTenant()
    {
        return new Model.Tenant
        {
            Id = Guid.NewGuid(),
            Name = "urn:fdc:gov.uk:2022:43af5a8b-f4c0-414b-b341-d4f1fa894302"
        };
    }
}