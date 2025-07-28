namespace CO.CDP.RegisterOfCommercialTools.WebApiClient;

public class CommercialToolsApiClient : ICommercialToolsApiClient
{
    private readonly HttpClient _httpClient;

    public CommercialToolsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}