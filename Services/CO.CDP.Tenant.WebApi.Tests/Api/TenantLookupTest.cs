using CO.CDP.OrganisationInformation;
using CO.CDP.Tenant.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;
using System.Net.Http.Json;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Tenant.WebApi.Tests.Api;

public class TenantLookupTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<TenantLookup?>> _getTenantUseCase = new();

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
        _getTenantUseCase.Setup(useCase => useCase.Execute())
            .ReturnsAsync((TenantLookup?)null);

        var response = await _httpClient.GetAsync($"/tenant/lookup");

        response.Should().HaveStatusCode(NotFound);
    }

    [Fact]
    public async Task IfTenantIsFound_ReturnsTenant()
    {
        var lookup = GivenTenantLookup();

        _getTenantUseCase.Setup(useCase => useCase.Execute())
            .ReturnsAsync(lookup);

        var response = await _httpClient.GetAsync("/tenant/lookup");

        response.Should().HaveStatusCode(OK);
        (await response.Content.ReadFromJsonAsync<TenantLookup>()).Should().BeEquivalentTo(lookup);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetSharedData_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel)
    {
        _getTenantUseCase.Setup(useCase => useCase.Execute())
            .ReturnsAsync(GivenTenantLookup());

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _getTenantUseCase.Object));

        var response = await factory.CreateClient().GetAsync("/tenant/lookup");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    private static TenantLookup GivenTenantLookup()
    {
        return new TenantLookup
        {
            User = new UserDetails
            {
                Urn = "urn:fdc:gov.uk:2022:43af5a8bf4c0414bb341d4f1fa894302",
                Name = "Alice Cooper",
                Email = "person@example.com"
            },
            Tenants =
            [
                new UserTenant
                {
                    Id = Guid.Parse("3317aa13-e3b8-4c95-a217-44be57117eb1"),
                    Name = "TrentTheTenant",
                    Organisations =
                    [
                        new UserOrganisation
                        {
                            Id = Guid.Parse("dfd0c5d3-0740-4be4-aa42-e42ec9c00bad"),
                            Name = "Acme Ltd",
                            Roles = [PartyRole.Tenderer],
                            Scopes = ["ADMIN"],
                            Uri = new Uri("file:///organisations/dfd0c5d3-0740-4be4-aa42-e42ec9c00bad")
                        }
                    ]
                }
            ]
        };
    }
}