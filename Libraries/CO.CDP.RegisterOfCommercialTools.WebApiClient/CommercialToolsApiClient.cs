using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using System.Net.Http.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient;

public class CommercialToolsApiClient : ICommercialToolsApiClient
{
    private readonly HttpClient _httpClient;

    public CommercialToolsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SearchResponse?> SearchAsync(SearchRequestDto request)
    {
        try
        {
            var queryString = ToQueryString(request);
            return await _httpClient.GetFromJsonAsync<SearchResponse>($"api/Search?{queryString}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private static string ToQueryString(SearchRequestDto dto)
    {
        var properties = from p in dto.GetType().GetProperties()
                         where p.GetValue(dto, null) != null
                         select $"{p.Name}={Uri.EscapeDataString(p.GetValue(dto, null)!.ToString()!)}";

        return string.Join("&", properties);
    }
}