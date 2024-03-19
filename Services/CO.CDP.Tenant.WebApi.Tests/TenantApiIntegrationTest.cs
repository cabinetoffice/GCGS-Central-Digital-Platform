using System.Net;
using System.Net.Http.Json;
using CO.CDP.Tenant.Persistence;
using CO.CDP.Tenant.WebApi.Api;
using CO.CDP.Tenant.WebApi.Tests.Fixtures;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace CO.CDP.Tenant.WebApi.Tests;

public class TenantApiIntegrationTest
{
    private readonly HttpClient _httpClient;

    public TenantApiIntegrationTest()
    {
        TestWebApplicationFactory<Program> factory = new(new InMemoryTenantRepository());
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ItRegistersNewTenant()
    {
        var response = await _httpClient.PostAsJsonAsync("/tenants", new NewTenant
        {
            Name = "TrentTheTenant",
            ContactInfo = new TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        });

        response.Should().HaveStatusCode(HttpStatusCode.Created);
        response.Should().MatchLocation("^/tenants/[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$");
        await response.Should().HaveContent(new Api.Tenant
        {
            Id = response.TenantId(),
            Name = "TrentTheTenant",
            ContactInfo = new TenantContactInfo
            {
                Email = "trent@example.com",
                Phone = "07925987654"
            }
        });
    }
}

internal static class HttpResponseMessageExtensions
{
    internal static Guid TenantId(this HttpResponseMessage response)
    {
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location?.OriginalString.Should().Match("/tenants/*");
        return Guid.Parse(response.Headers.Location?.OriginalString.Replace("/tenants/", "") ?? "");
    }
}

internal static class AssertionExtensions
{
    internal static void MatchLocation(this HttpResponseMessageAssertions assertions, string regex)
    {
        using (new AssertionScope("Location header"))
        {
            assertions.Subject.Headers.Location.Should().NotBeNull();
            assertions.Subject.Headers.Location?.OriginalString.Should().MatchRegex(regex);
        }
    }

    internal static async Task HaveContent<T>(this HttpResponseMessageAssertions assertions, T content)
    {
        using (new AssertionScope("Content"))
        {
            (await assertions.Subject.Content.ReadFromJsonAsync<T>()).Should().Be(content);
        }
    }
}