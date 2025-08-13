using Xunit;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Tests;

public class CommercialToolsApiClientTest
{
    [Fact]
    public void ItInstantiates()
    {
        var httpClient = new HttpClient();
        var client = new CommercialToolsApiClient(httpClient);
        Assert.NotNull(client);
    }
}