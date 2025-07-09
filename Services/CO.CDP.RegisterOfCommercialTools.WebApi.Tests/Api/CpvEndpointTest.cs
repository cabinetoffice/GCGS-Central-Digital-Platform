using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Api;

public class CpvEndpointTest
{
    private readonly HttpClient _httpClient;

    public CpvEndpointTest()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>();
        _httpClient = webApplicationFactory.CreateClient();
    }

    [Fact]
    public async Task SearchCpv_ReturnsNotImplemented()
    {
        var response = await _httpClient.GetAsync("/filters/cpv/search");
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetCpvChildren_ReturnsNotImplemented()
    {
        var response = await _httpClient.GetAsync("/filters/cpv/12345678/children");
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }
}
