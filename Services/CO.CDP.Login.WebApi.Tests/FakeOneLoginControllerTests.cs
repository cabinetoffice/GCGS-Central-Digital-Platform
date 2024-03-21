using FluentAssertions;
using CO.CDP.Common.Auth;
using System.Net;
using System.Net.Http.Json;

namespace CO.CDP.Login.WebApi.Tests;

public class FakeOneLoginControllerTests
{
    private readonly HttpClient _client;

    public FakeOneLoginControllerTests()
    {
        _client = new HttpClient();
    }

    [Fact]
    public async Task Get_HealthyEndpoint_ReturnsSuccess()
    {
        string url = "http://localhost:5130/fake-onelogin";
        HttpResponseMessage response = await _client.GetAsync(url);
        var result = await response.Content.ReadFromJsonAsync<OneLoginResponce>();

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result?.Sub.Should().Be("urn:fdc:gov.uk:2022:56P4CMsGh_-2sVIB2nsNU7mcLZYhYw=");
        result?.Email.Should().Be("test@example.com");
        result?.EmailVerified.Should().BeTrue();
        result?.PhoneNumber.Should().Be("01406946277");
        result?.PhoneNumberVerified.Should().BeTrue();
        result?.UpdatedAt.Should().Be(1311280970);
    }

}