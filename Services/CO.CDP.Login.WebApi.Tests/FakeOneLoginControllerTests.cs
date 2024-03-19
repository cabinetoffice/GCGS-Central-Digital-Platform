using System.Net;
using System.Net.Http;
using Xunit;
using System.Text.Json;
using CO.CDP.Common.Auth;

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
        string contentString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OneLoginResponce>(contentString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("urn:fdc:gov.uk:2022:56P4CMsGh_-2sVIB2nsNU7mcLZYhYw=", result.Sub);
        Assert.Equal("test@example.com", result.Email);
        Assert.True(result.EmailVerified);
        Assert.Equal("01406946277", result.PhoneNumber);
        Assert.True(result.PhoneNumberVerified);
        Assert.Equal(1311280970, result.UpdatedAt);
    }

}