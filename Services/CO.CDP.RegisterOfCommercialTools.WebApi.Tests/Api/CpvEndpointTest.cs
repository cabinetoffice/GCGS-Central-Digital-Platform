using System.Net;
using System.Net.Http.Json;
using CO.CDP.RegisterOfCommercialTools.WebApi.UseCases;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Api;

public class CpvEndpointTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<ICpvCodeRepository> _repositoryMock = new();

    public CpvEndpointTest()
    {
        var webApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => { services.AddSingleton(_repositoryMock.Object); });
            });
        _httpClient = webApplicationFactory.CreateClient();
    }

    [Fact]
    public async Task SearchCpv_ReturnsNotImplemented()
    {
        var response = await _httpClient.GetAsync("/filters/cpv/search");
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetCpvChildren_WhenCodeIsValid_ReturnsChildren()
    {
        var cpvCodes = new List<CpvCode>
        {
            new("03000000-1", "Agricultural, farming, fishing, forestry and related products"),
            new("03100000-2", "Agricultural and horticultural products"),
            new("03200000-3", "Cereals, potatoes, vegetables, fruits and nuts"),
            new("03300000-4", "Farming, hunting and fishing products"),
            new("03400000-5", "Forestry and logging products"),
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cpvCodes);

        var response = await _httpClient.GetAsync("/filters/cpv/03000000/children");
        response.EnsureSuccessStatusCode();

        var children = await response.Content.ReadFromJsonAsync<List<CpvCode>>();

        Assert.NotNull(children);

        var expectedCodes = new[] { "03100000-2", "03200000-3", "03300000-4", "03400000-5" };
        Assert.Equal(expectedCodes, children.Select(c => c.Code).ToList());
    }
}
