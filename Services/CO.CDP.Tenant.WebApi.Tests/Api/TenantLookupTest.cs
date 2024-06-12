using System.Net.Http.Json;
using CO.CDP.OrganisationInformation;
using CO.CDP.Tenant.WebApi.Model;
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
    private readonly Mock<IUseCase<string, TenantLookup?>> _getTenantUseCase = new();

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
            .Returns(Task.FromResult<TenantLookup?>(null));

        var response = await _httpClient.GetAsync($"/tenant/lookup?urn={urn}");

        response.Should().HaveStatusCode(NotFound);
    }

    [Fact]
    public async Task IfTenantIsFound_ReturnsTenant()
    {
        var userUrn = "urn:fdc:gov.uk:2022:43af5a8bf4c0414bb341d4f1fa894302";
        var lookup = GivenTenantLookup(
            userUrn: userUrn
        );

        _getTenantUseCase.Setup(useCase => useCase.Execute(userUrn))
            .Returns(Task.FromResult<TenantLookup?>(lookup));

        var response = await _httpClient.GetAsync($"/tenant/lookup?urn={userUrn}");

        response.Should().HaveStatusCode(OK);
        (await response.Content.ReadFromJsonAsync<TenantLookup>()).Should().BeEquivalentTo(lookup);
    }

    private static TenantLookup GivenTenantLookup(
        string userUrn = "urn:fdc:gov.uk:2022:43af5a8bf4c0414bb341d4f1fa894302"
    )
    {
        return new TenantLookup
        {
            User = new UserDetails
            {
                Urn = userUrn,
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
                            Roles = [PartyRole.Supplier],
                            Scopes = ["ADMIN"],
                            Uri = new Uri("file:///organisations/dfd0c5d3-0740-4be4-aa42-e42ec9c00bad")
                        }
                    ]
                }
            ]
        };
    }
}
